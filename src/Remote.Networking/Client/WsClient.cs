using System.Net;
using System.Reactive;
using System.Reactive.Subjects;
using NetCoreServer;

namespace RaceDirector.Remote.Networking.Client;

public class WsClient<TOut, TIn> : IWsClient<TOut, TIn>
{
    /// <summary>
    /// Latest version specified in the WebSocket Protocol RFC6455
    /// https://www.iana.org/assignments/websocket/websocket.xml#version-number
    /// </summary>
    private const int WsProtocolVersion = 13;

    // FIXME it can disconnect after connection
    public Task Connected => _connectedCompletionSource.Task;
    public IObserver<TOut> Out { get; }
    public IObservable<TIn> In { get; }

    private WsClient _inner;
    private readonly Codec<TOut, TIn> _codec;
    private readonly TaskCompletionSource _connectedCompletionSource;
    private readonly string _path;

    public bool Connect() => _inner.ConnectAsync();

    public bool Disconnect() => _inner.Disconnect();
    
    public void Dispose() => _inner.Dispose();

    public event MessageHandler<TIn>? MessageHandler;

    public WsClient(string uri, Codec<TOut, TIn> codec) : this(new Uri(uri), codec) { }

    /// <summary>
    /// Initialises a WebSocket client, without connecting.
    /// </summary>
    /// <param name="uri">Server URI with host, port and path.</param>
    /// <param name="codec">Encoder/decoder to/from binary messages.</param>
    public WsClient(Uri uri, Codec<TOut, TIn> codec)
    {
        if (uri.Scheme != Uri.UriSchemeWs)
            throw new NotSupportedException($"Protocol {uri.Scheme} is not supported. Use {Uri.UriSchemeWs} instead.");
        _inner = CreateInnerClient(uri);
        _codec = codec;
        _connectedCompletionSource = new TaskCompletionSource();
        _path = uri.AbsolutePath;

        var subject = new Subject<TIn>();
        MessageHandler += i => subject.OnNext(i);
        In = subject;
        Out = Observer.Create<TOut>(_ => WsSendAsync(_));
    }

    private InnerClient CreateInnerClient(Uri uri)
    {
        if (IPAddress.TryParse(uri.Host, out var address))
            return new InnerClient(this, new IPEndPoint(address, uri.Port));            
        return new InnerClient(this, new DnsEndPoint(uri.Host, uri.Port));
    }

    public bool WsSendAsync(TOut message)
    {
        var payload = _codec.Encode(message);
        return payload.IsEmpty || _inner.SendTextAsync(payload.Span);
    }

    private class InnerClient : WsClient
    {
        private readonly WsClient<TOut, TIn> _outer;

        internal InnerClient(WsClient<TOut, TIn> outer, DnsEndPoint dnsEndPoint) : base(dnsEndPoint)
        {
            _outer = outer;
        }
        
        internal InnerClient(WsClient<TOut, TIn> outer, IPEndPoint ipEndPoint) : base(ipEndPoint)
        {
            _outer = outer;
        }

        public override void OnWsConnecting(HttpRequest request)
        {
            request.SetBegin(HttpMethod.Get.Method, _outer._path);
            request.SetHeader("Connection", "Upgrade");
            request.SetHeader("Upgrade", "websocket");
            request.SetHeader("Sec-WebSocket-Key", Convert.ToBase64String(WsNonce));
            request.SetHeader("Sec-WebSocket-Version", WsProtocolVersion.ToString());
            request.SetBody();
        }

        public override void OnWsConnected(HttpResponse response)
        {
            _outer._connectedCompletionSource.SetResult();
        }

        public override void OnWsReceived(byte[] buffer, long offset, long size)
        {
            var payload = new ReadOnlyMemory<byte>(buffer, Convert.ToInt32(offset), Convert.ToInt32(size));
            var message = _outer._codec.Decode(payload);
            _outer.MessageHandler?.Invoke(message);
        }        
    }
}