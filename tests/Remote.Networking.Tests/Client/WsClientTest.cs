using System.Net;
using RaceDirector.Remote.Networking;
using RaceDirector.Remote.Networking.Client;
using Xunit;
using Xunit.Categories;

namespace Remote.Networking.Tests.Client;

[UnitTest]
public class WsClientTest
{
    [Fact]
    public void RequiresProtocol()
    {
        Assert.Throws<UriFormatException>(() =>
        {
            var client = new WsClient<Nothing, Nothing>(IPAddress.Loopback.ToString(), Codec.Nothing);
        });
    }

    [Fact]
    public void AcceptsIpAddress()
    {
        var client = new WsClient<Nothing, Nothing>($"ws://{IPAddress.Loopback}", Codec.Nothing);
    }
    
    [Fact]
    public void AcceptsHostname()
    {
        var client =
            new WsClient<Nothing, Nothing>($"ws://localhost", Codec.Nothing);
    }
    
    [Fact]
    public void RefusesToSendWhenDisconnected()
    {
        var client =
            new WsClient<string, string>($"ws://{IPAddress.Loopback}", Codec.UTF8String);
        Assert.False(client.WsSendAsync("foo"));
    }
}