using HUD.Tests.Base;
using HUD.Tests.TestUtils;
using RaceDirector.Plugin.HUD.Pipeline;
using RaceDirector.Plugin.HUD.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Xunit;

namespace HUD.Tests.Pipeline
{
    public class MultiEndpointWsServerTest : IntegrationTestBase
    {
        [Fact]
        public void FailsConnectionIfNoMatchingEndpoint()
        {
            WithServerClient(Enumerable.Empty<IEndpoint<int>>(), "GET", (server, client) => {
                Assert.False(client.ConnectAndWait());
            });
        }

        [Fact]
        public void ReceivedBroadcastForMatchingEndpoint()
        {
            var endpoints = new [] {
                new Endpoint<int>(Endpoint.PathMatcher("/foo"), _ => Encoding.UTF8.GetBytes("")),
                new Endpoint<int>(Endpoint.PathMatcher("/bar"), i => Encoding.UTF8.GetBytes(i.ToString())),
                new Endpoint<int>(Endpoint.PathMatcher("/baz"), _ => Encoding.UTF8.GetBytes("24")),
            };
            WithServerClient(endpoints, "/bar", (server, client) =>
            {
                Assert.True(client.ConnectAndWait());
                server.Multicast(42);
                Assert.Equal("42", client.nextString());
            });
        }

        #region Test setup

        private void WithServerClient<T>(IEnumerable<IEndpoint<T>> endpoints, string path, Action<IWsServer<T>, JsonWsClient> action)
        {
            var serverPort = Tcp.FreePort();
            using (var server = new MultiEndpointWsServer<T>(IPAddress.Any, serverPort, endpoints))
            {
                server.Start();
                using (var client = new JsonWsClient(Timeout, serverPort, path))
                {
                    action(server, client);
                }
            }
        }
        #endregion
    }
}
