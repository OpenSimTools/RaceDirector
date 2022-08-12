using RaceDirector.Remote.Networking.Utils;
using RaceDirector.Remote.Networking.Server;
using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using RaceDirector.Remote.Networking.Client;
using Xunit;
using Xunit.Categories;

namespace Remote.Networking.Tests.Server;

[IntegrationTest]
public class MultiEndpointWsServerTest
{
    private static TimeSpan Timeout { get => TimeSpan.FromMilliseconds(500); }

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
            Assert.True(client.ConnectAndWait(), "Client could not connect");
            server.Multicast(42);
            Assert.Equal("42", client.NextString());
        });
    }

    #region Test setup

    private void WithServerClient<T>(IEnumerable<IEndpoint<T>> endpoints, string path, Action<IWsServer<T>, JsonWsClient> action)
    {
        var serverPort = Tcp.FreePort();
        using var server = new MultiEndpointWsServer<T>(IPAddress.Any, serverPort, endpoints, NullLogger.Instance);
        server.Start();
        using var client = new JsonWsClient(Timeout, serverPort, path);
        action(server, client);
    }
    #endregion
}