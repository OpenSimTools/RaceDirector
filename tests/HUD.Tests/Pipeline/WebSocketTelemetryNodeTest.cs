using NetCoreServer;
using RaceDirector.Plugin.HUD.Pipeline;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks.Dataflow;
using Xunit;
using Xunit.Categories;

namespace HUD.Tests.Pipeline
{
    [IntegrationTest]
    public class WebSocketTelemetryNodeTest
    {
        private int serverPort = FreeTcpPort();

        [Fact]
        public void WsServerIsUpWhenGameRunning()
        {
            using (var node = new WebSocketTelemetryNode(
                new WebSocketTelemetryNode.Config(serverPort),
                Enumerable.Empty<ITelemetryEndpoint>()
            ))
            {
                using (var client = new Client(serverPort))
                {
                    Assert.False(client.Connect());
                    node.RunningGameTarget.Post(new RunningGame("any"));
                    Assert.True(client.Connect());
                    node.RunningGameTarget.Post(new RunningGame(null));
                    Assert.False(client.Reconnect());
                }
            }
        }

        static int FreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

        private class Client : WsClient
        {
            public Client(int port) : base(IPAddress.Loopback, port) { }
        }

        private record RunningGame(string? Name) : RaceDirector.Pipeline.GameMonitor.V0.IRunningGame;
    }
}
