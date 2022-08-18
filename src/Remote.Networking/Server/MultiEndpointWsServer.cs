using NetCoreServer;
using System.Net;
using Microsoft.Extensions.Logging;

namespace RaceDirector.Remote.Networking.Server;

/// <summary>
/// WebSocket server that serialises messages differently based on the endpoint that was requested.
/// </summary>
/// <typeparam name="TIn">Type of message that can be received from clients.</typeparam>
/// <typeparam name="TOut">Type of message that can be sent to client.</typeparam>
/// <remarks>NetCoreServer is poorly designed for extensibility. This class could be a lot simpler.</remarks>
public class MultiEndpointWsServer<TOut, TIn> : IWsServer<TOut, TIn>
{
    private readonly InnerServer _inner;
    private readonly Uri _baseUri;
    private readonly List<HttpEndpoint<TOut, TIn>> _endpoints;
    private readonly ILogger _logger;

    public event MessageHandler<TIn>? MessageHandler;

    public MultiEndpointWsServer(IPAddress address, int port, IEnumerable<HttpEndpoint<TOut, TIn>> endpoints, ILogger logger)
    {
        _inner = new InnerServer(this, address, port);
        _baseUri = new UriBuilder(Uri.UriSchemeWs, address.ToString(), port).Uri;
        _endpoints = endpoints.ToList();
        _logger = logger;
    }

    public bool Start() => _inner.Start();

    public bool Stop() => _inner.Stop();

    public void Dispose() => _inner.Dispose();

    public void WsMulticastAsync(TOut message) =>
        _inner.WsMulticastAsync(message, _ => true);

    public void WsMulticastAsync(TOut message, Func<ISession, bool> condition) =>
        _inner.WsMulticastAsync(message, condition);

    private class InnerServer : WsServer
    {
        private readonly MultiEndpointWsServer<TOut, TIn> _outer;

        internal InnerServer(MultiEndpointWsServer<TOut, TIn> outer, IPAddress address, int port) : base(address, port)
        {
            _outer = outer;
        }
        
        protected sealed override TcpSession CreateSession()
        {
            return new InnerSession(_outer);
        }
        
        internal bool WsMulticastAsync(TOut message, Func<ISession, bool> condition)
        {
            if (!IsStarted)
            {
                _outer._logger.LogTrace("Server stopped; skipping message {T}", message);
                return false;
            }

            _outer._logger.LogTrace("Sending message {T}", message);
            var ret = true;
            foreach (var session in Sessions.Values)
            {
                if (session is not InnerSession innerSession) continue;
                if (condition(innerSession))
                    ret &= innerSession.WsSendAsync(message);
            }

            return ret;
        }
        
        protected override void OnStarted()
        {
            _outer._logger.LogInformation("Server started");
        }
        
        protected override void OnStopped()
        {
            _outer._logger.LogInformation("Server stopped");
        }
    }
    
    private class InnerSession : WsSession, ISession
    {
        private readonly MultiEndpointWsServer<TOut, TIn> _outer;
        private HttpEndpoint<TOut, TIn>? _matchedEndpoint;
        private bool _wsConnected;

        object ISession.Id => base.Id;

        internal InnerSession(MultiEndpointWsServer<TOut, TIn> outer) : base(outer._inner)
        {
            _outer = outer;
            _wsConnected = false;
        }

        public override bool OnWsConnecting(HttpRequest innerRequest, HttpResponse _)
        {
            var request = new HttpRequestWrapper(_outer._baseUri, innerRequest);
            _matchedEndpoint = _outer._endpoints.Find(e => e.Matcher(request));
            return _matchedEndpoint != null;
        }

        public override void OnWsConnected(HttpRequest request) {
            _outer._logger.LogDebug("Client connected");
            _wsConnected = true;
        }

        public sealed override void OnWsReceived(byte[] buffer, long offset, long size)
        {
            if (_matchedEndpoint is null)
                return;
            var payload = new ReadOnlyMemory<byte>(buffer, Convert.ToInt32(offset), Convert.ToInt32(size));
            var message = _matchedEndpoint.Codec.Decode(payload);
            _outer.MessageHandler?.Invoke(this, message);
        }

        public bool WsSendAsync(TOut t)
        {
            if (!_wsConnected || _matchedEndpoint is null)
                return false;
            var payload = _matchedEndpoint.Codec.Encode(t);
            return SendTextAsync(payload.Span);
        }
    }
}