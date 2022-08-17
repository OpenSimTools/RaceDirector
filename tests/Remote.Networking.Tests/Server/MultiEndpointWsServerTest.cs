using RaceDirector.Remote.Networking.Server;
using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using RaceDirector.Remote.Networking;
using RaceDirector.Remote.Networking.Client;
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
        WithServerClient(Enumerable.Empty<HttpEndpoint<int, Nothing>>(), "/", (_, client) => {
            Assert.False(client.ConnectAndWait());
        });
    }

    [Fact]
    public void ReceivedBroadcastForMatchingEndpoint()
    {
        var endpoints = new HttpEndpoint<int, Nothing>[] {
            new(HttpEndpoint.PathMatcher("/foo"), ServerCodec(_ => "")),
            new(HttpEndpoint.PathMatcher("/bar"), ServerCodec(i => i.ToString())),
            new(HttpEndpoint.PathMatcher("/baz"), ServerCodec(_ => "24"))
        };
        WithServerClient(endpoints, "/bar", (server, client) =>
        {
            Assert.True(client.ConnectAndWait(), "Client could not connect");
            server.WsMulticastAsync(42);
            Assert.Equal("42", client.Next());
        });
    }

    #region Test setup

    private void WithServerClient(IEnumerable<HttpEndpoint<int, Nothing>> endpoints, string path, Action<IWsServer<int, Nothing>, SyncWsClient<Nothing, string>> action)
    {
        var serverPort = Tcp.FreePort();
        using var server = new MultiEndpointWsServer<int, Nothing>(IPAddress.Any, serverPort, endpoints, NullLogger.Instance);
        server.Start();
        using var client = new SyncWsClient<Nothing, string>($"ws://{IPAddress.Loopback}:{serverPort}{path}", ClientCodec, Timeout);
        action(server, client);
    }

    private static Codec<int, Nothing> ServerCodec(Func<int, string> f) =>
        Codec.EncodeOnly<int>(i => Encoding.UTF8.GetBytes(f(i)));
    
    private static readonly Codec<Nothing, string> ClientCodec =
        Codec.DecodeOnly(Codec.UTF8String.Decode);

    #endregion
}