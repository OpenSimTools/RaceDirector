using NetCoreServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Logging;

namespace RaceDirector.Plugin.HUD.Server
{
    /// <summary>
    /// WebSocket server that serialises messages differently based on the endpoint that was requested.
    /// </summary>
    /// <typeparam name="T">Type of message that can be broadcasted.</typeparam>
    /// <remarks>NetCoreServer is poorly designed for extensibility. This class could be a lot simpler.</remarks>
    public class MultiEndpointWsServer<T> : WsServer, IWsServer<T>
    {
        private readonly List<IEndpoint<T>> _endpoints;
        protected readonly ILogger Logger;

        public MultiEndpointWsServer(IPAddress address, int port, IEnumerable<IEndpoint<T>> endpoints, ILogger logger)
            : base(address, port)
        {
            _endpoints = endpoints.ToList();
            Logger = logger;
        }

        protected override TcpSession CreateSession()
        {
            return new MultiEndpointWsSession(this);
        }
        
        /// <summary>
        /// Multicasts data to all clients connected. Serialisation depends on the endpoint that
        /// clients are connected to.
        /// </summary>
        /// <param name="t">Message</param>
        public bool Multicast(T t)
        {
            if (!IsStarted)
            {
                Logger.LogTrace("Server stopped; skipping message {T}", t);
                return false;
            }

            Logger.LogTrace("Sending message {T}", t);
            foreach (var session in Sessions.Values)
            {
                if (session is MultiEndpointWsSession mewsSession)
                {
                    mewsSession.SendAsync(t);
                }
            }

            return true;
        }

        private class MultiEndpointWsSession : WsSession
        {
            private readonly MultiEndpointWsServer<T> _server;
            private IEndpoint<T>? _matchedEndpoint;
            private bool _wsHandshaked;

            public MultiEndpointWsSession(MultiEndpointWsServer<T> server) : base(server)
            {
                _server = server;
                _wsHandshaked = false;
            }

            public override bool OnWsConnecting(HttpRequest request, HttpResponse response) {
                _matchedEndpoint = _server._endpoints.Find(e => e.Matches(request));
                return _matchedEndpoint != null;
            }

            public override void OnWsConnected(HttpRequest request) {
                _server.Logger.LogDebug("Client connected");
                _wsHandshaked = true;
            }

            public bool SendAsync(T t)
            {
                if (_wsHandshaked && _matchedEndpoint != null)
                    return SendTextAsync(_matchedEndpoint.Transform(t));
                else
                    return false;
            }

            private bool SendTextAsync(byte[] buffer)
            {
                return SendTextAsync(buffer, 0L, buffer.LongLength);
            }
        }
    }
}
