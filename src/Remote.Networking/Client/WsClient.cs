using NetCoreServer;
using RaceDirector.Remote.Networking.Codec;

namespace RaceDirector.Remote.Networking.Client;

public abstract class WsClient<TOut, TIn> : NetCoreServer.WsClient, IWsClient<TOut>
{
    public Task Connected => _connectedCompletionSource.Task;

    private readonly ICodec<TOut, TIn> _codec;
    private readonly TaskCompletionSource _connectedCompletionSource;
    private readonly string _path;

    public WsClient(string uri, ICodec<TOut, TIn> codec) : this(new Uri(uri), codec) { }

    /// <summary>
    /// Initialises a WebSocket client, without connecting.
    /// </summary>
    /// <param name="uri">Server URI with host, port and path.</param>
    /// <param name="codec">Encoder/decoder to/from binary messages.</param>
    public WsClient(Uri uri, ICodec<TOut, TIn> codec) : base(uri.Host, uri.Port)
    {
        if (uri.Scheme != Uri.UriSchemeWs)
            throw new NotSupportedException($"Protocol {uri.Scheme} is not supported. Use {Uri.UriSchemeWs} instead.");
        _codec = codec;
        _connectedCompletionSource = new TaskCompletionSource();
        _path = uri.AbsolutePath;
    }

    public override void OnWsConnecting(HttpRequest request)
    {
        request.SetBegin(HttpMethod.Get.Method, _path);
        request.SetHeader("Connection", "Upgrade");
        request.SetHeader("Upgrade", "websocket");
        request.SetHeader("Sec-WebSocket-Key", Convert.ToBase64String(WsNonce));
        request.SetHeader("Sec-WebSocket-Version", "13");
        request.SetBody();
    }

    public override void OnWsConnected(HttpResponse response)
    {
        _connectedCompletionSource.SetResult();
    }

    public override void OnWsReceived(byte[] buffer, long offset, long size)
    {
        var payload = new ReadOnlySpan<byte>(buffer, Convert.ToInt32(offset), Convert.ToInt32(size));
        var message = _codec.Decode(payload);
        OnWsReceived(message);
    }

    protected virtual void OnWsReceived(TIn message) { }

    public bool WeSendAsync(TOut message)
    {
        var payload = _codec.Encode(message);
        return SendTextAsync(payload);
    }
}