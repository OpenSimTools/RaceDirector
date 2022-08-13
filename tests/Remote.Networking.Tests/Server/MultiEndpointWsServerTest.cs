using RaceDirector.Remote.Networking.Server;
using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using RaceDirector.Remote.Networking;
using RaceDirector.Remote.Networking.Client;
using RaceDirector.Remote.Networking.Codec;
using TestUtils;
using Xunit;
using Xunit.Categories;

namespace Remote.Networking.Tests.Server;

[IntegrationTest]
public class MultiEndpointWsServerTest
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMilliseconds(500);

    [Fact]
    public void FailsConnectionIfNoMatchingEndpoint()
    {
        WithServerClient(Enumerable.Empty<IEndpoint<int, Nothing>>(), "/", (_, client) => {
            Assert.False(client.ConnectAndWait());
        });
    }

    [Fact]
    public void ReceivedBroadcastForMatchingEndpoint()
    {
        var endpoints = new [] {
            new Endpoint<int, Nothing>(Endpoint.PathMatcher("/foo"), TestCodec(_ => "")),
            new Endpoint<int, Nothing>(Endpoint.PathMatcher("/bar"), TestCodec(i => i.ToString())),
            new Endpoint<int, Nothing>(Endpoint.PathMatcher("/baz"), TestCodec(_ => "24"))
        };
        WithServerClient(endpoints, "/bar", (server, client) =>
        {
            Assert.True(client.ConnectAndWait(), "Client could not connect");
            server.WsMulticastAsync(42);
            Assert.Equal("42", client.Next());
        });
    }

    #region Test setup

    private void WithServerClient(IEnumerable<IEndpoint<int, Nothing>> endpoints, string path, Action<IWsServer<int>, SyncWsClient<string, string>> action)
    {
        var serverPort = Tcp.FreePort();
        using var server = new MultiEndpointWsServer<int, Nothing>(IPAddress.Any, serverPort, endpoints, NullLogger.Instance);
        server.Start();
        using var client = new SyncWsClient<string, string>($"ws://{IPAddress.Loopback}:{serverPort}{path}", StringCodec.UTF8, Timeout);
        action(server, client);
    }

    private ICodec<int, Nothing> TestCodec(Func<int, string> f) => Encoder<int>.From(i => Encoding.UTF8.GetBytes(f(i))).ToCodec();

    #endregion
}