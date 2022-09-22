using System.Net;
using System.Reactive.Linq;
using RaceDirector.Remote.Networking;
using RaceDirector.Remote.Networking.Client;
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
    
    [Fact]
    public void CanSendMessages()
    {
        using var server = new EchoServer();
        Assert.True(server.Start());

        using var client = new SyncWsClient<string, string>($"ws://{IPAddress.Loopback}:{server.Port}", Codec.UTF8String, Timeout);
        Assert.True(client.ConnectAndWait());
        Observable.Range(0, 3).Select(_ => _.ToString())
            .Subscribe(client.Out);

        Eventually(() => Assert.Equal(3, client.Available())).Within(Timeout);
        Assert.Equal("<- 0", client.Next());
        Assert.Equal("<- 1", client.Next());
        Assert.Equal("<- 2", client.Next());
    }
    
    [Fact]
    public void CanThrottleMessages()
    {
        using var server = new EchoServer();
        Assert.True(server.Start());

        var throttling = TimeSpan.FromMilliseconds(100);
        using var client = new SyncWsClient<string, string>($"ws://{IPAddress.Loopback}:{server.Port}", Codec.UTF8String, throttling, Timeout);
        Assert.True(client.ConnectAndWait());
        Observable.Interval(TimeSpan.FromMilliseconds(45)).Select(_ => (2 * (_ / 2)).ToString())
            .Take(6)
            .Subscribe(client.Out);

        Eventually(() => Assert.Equal(3, client.Available())).Within(Timeout);
        Assert.Equal("<- 0", client.Next());
        Assert.Equal("<- 2", client.Next());
        Assert.Equal("<- 4", client.Next());
    }
}