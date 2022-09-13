using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RaceDirector.Remote.Networking;
using RaceDirector.Remote.Networking.Client;
using RaceDirector.Remote.Networking.Server;
using TestUtils;
using Xunit;
using Xunit.Categories;

namespace PitCrew.Server.Tests;

[IntegrationTest]
public class PitCrewServerTest
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMilliseconds(500);

    [Fact]
    public void BroadcastsToOtherConnectedClients()
    {
        WithServer(serverPort =>
        {
            using var client1 = ConnectedClient(serverPort);
            using var client2 = ConnectedClient(serverPort);
            client1.WsSendAsync("c1");
            client2.WsSendAsync("c2");
            Assert.Equal("c1", client2.Next());
            Assert.Equal("c2", client1.Next());
        });
    }
    
    private static void WithServer(Action<int> action)
    {
        var serverPort = Tcp.FreePort();
        using var server = new PitCrewServer(new Config { Port = serverPort }, NullLogger<PitCrewServer>.Instance);
        Assert.True(server.Start());
        action(serverPort);
    }

    private static SyncWsClient<string, string> ConnectedClient(int serverPort)
    {
        var client = new SyncWsClient<string, string>($"ws://{IPAddress.Loopback}:{serverPort}", Codec.UTF8String, Timeout);
        Assert.True(client.ConnectAndWait());
        return client;
    }
}