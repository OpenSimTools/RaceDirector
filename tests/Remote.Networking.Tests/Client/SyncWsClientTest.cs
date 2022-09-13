using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using RaceDirector.Remote.Networking;
using RaceDirector.Remote.Networking.Client;
using RaceDirector.Remote.Networking.Server;
using TestUtils;
using Xunit;
using Xunit.Categories;
using static TestUtils.EventuallyAssertion;

namespace Remote.Networking.Tests.Client;

// This is really testing WsClient, but using SyncWsClient to avoid reimplementing the same methods in tests.
[IntegrationTest]
public class SyncWsClientTest
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMilliseconds(500);

    [Fact]
    public void DoesNotSendEmptyPayloads()
    {
        using var server = new EchoServer();
        Assert.True(server.Start());

        using var client = new SyncWsClient<string, string>($"ws://{IPAddress.Loopback}:{server.Port}", Codec.UTF8String, Timeout);
        Assert.True(client.ConnectAndWait());
        Assert.True(client.WsSendAsync(""));
        Assert.True(client.WsSendAsync("not empty"));

        Eventually(() => Assert.True(client.NextIsAvailable())).Within(Timeout);
        Assert.Equal("<- not empty", client.Next());
    }
    
    #region Test Setup
    
    private class EchoServer : MultiEndpointWsServer<string, string>
    {
        private static readonly HttpEndpoint<string, string>[] Endpoint =
        {
            new(_ => true, Codec.UTF8String)
        };

        public EchoServer() :
            base(IPAddress.Any, Tcp.FreePort(), Endpoint, NullLogger.Instance)
        {
            // The server would refuse to send an empty message, so we need to make
            // sure that empty messages from the client are still sent back.
            MessageHandler += (session, message) => session.WsSendAsync($"<- {message}");
        }
    }
    
    #endregion
}