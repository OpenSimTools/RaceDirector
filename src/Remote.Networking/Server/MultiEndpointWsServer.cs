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
public class MultiEndpointWsServer<TOut, TIn> : WsServer, IWsServer<TOut>
{
    private readonly List<IEndpoint<TOut, TIn>> _endpoints;
    protected readonly ILogger Logger;

    public MultiEndpointWsServer(IPAddress address, int port, IEnumerable<IEndpoint<TOut, TIn>> endpoints, ILogger logger)
        : base(address, port)
    {
        _endpoints = endpoints.ToList();
        Logger = logger;
    }

    protected sealed override TcpSession CreateSession()
    {
        return new MultiEndpointWsSession(this);
    }
    
    /// <summary>
    /// Override this to handle incoming messages from a session.
    /// </summary>
    /// <param name="session">Session that received the message</param>
    /// <param name="message">Message received</param>
    protected virtual void OnWsReceived(WsSession session, TIn message) { }
        
    /// <summary>
    /// Multicasts data to all clients connected. Serialisation depends on the endpoint that
    /// clients are connected to.
    /// </summary>
    /// <param name="message">Message</param>
    public bool WsMulticastAsync(TOut message)
    {
        return WsMulticastAsync(message, _ => true);
    }

    /// <summary>
    /// Multicasts data to all connected clients matching a condition.
    /// Serialisation depends on the endpoint that clients are connected to.
    /// </summary>
    /// <param name="message">Message</param>
    /// <param name="condition">Should this session be sent the message?</param>
    public bool WsMulticastAsync(TOut message, Func<WsSession, bool> condition)
    {
        if (!IsStarted)
        {
            Logger.LogTrace("Server stopped; skipping message {T}", message);
            return false;
        }

        Logger.LogTrace("Sending message {T}", message);
        var ret = true;
        foreach (var session in Sessions.Values)
        {
            if (session is not MultiEndpointWsSession mewsSession) continue;
            if (condition(mewsSession))
                ret &= mewsSession.WsSendAsync(message);
        }

        return ret;
    }

    private class MultiEndpointWsSession : WsSession
    {
        private readonly MultiEndpointWsServer<TOut, TIn> _server;
        private IEndpoint<TOut, TIn>? _matchedEndpoint;
        private bool _wsConnected;

        public MultiEndpointWsSession(MultiEndpointWsServer<TOut, TIn> server) : base(server)
        {
            _server = server;
            _wsConnected = false;
        }

        public override bool OnWsConnecting(HttpRequest request, HttpResponse response) {
            _matchedEndpoint = _server._endpoints.Find(e => e.Matches(request));
            return _matchedEndpoint != null;
        }

        public override void OnWsConnected(HttpRequest request) {
            _server.Logger.LogDebug("Client connected");
            _wsConnected = true;
        }

        public sealed override void OnWsReceived(byte[] buffer, long offset, long size)
        {
            if (_matchedEndpoint is null)
                return;
            var payload = new ReadOnlyMemory<byte>(buffer, Convert.ToInt32(offset), Convert.ToInt32(size));
            var message = _matchedEndpoint.Codec.Decode(payload);
            _server.OnWsReceived(this, message);
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