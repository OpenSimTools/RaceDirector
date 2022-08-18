using System.Net;
using RaceDirector.Remote.Networking;
using RaceDirector.Remote.Networking.Client;
using Xunit;
using Xunit.Categories;

namespace Remote.Networking.Tests.Client;

[IntegrationTest]
public class SyncWsClientTest
{
    [Fact]
    public void RequiresProtocol()
    {
        Assert.Throws<UriFormatException>(() =>
        {
            var client = new SyncWsClient<Nothing, Nothing>(IPAddress.Loopback.ToString(), Codec.Nothing, TimeSpan.Zero);
        });
    }

    [Fact]
    public void AcceptsIpAddress()
    {
        var client = new SyncWsClient<Nothing, Nothing>($"ws://{IPAddress.Loopback}", Codec.Nothing, TimeSpan.Zero);
    }
    
    [Fact]
    public void AcceptsHostname()
    {
        var client =
            new SyncWsClient<Nothing, Nothing>($"ws://localhost", Codec.Nothing, TimeSpan.Zero);
    }
}