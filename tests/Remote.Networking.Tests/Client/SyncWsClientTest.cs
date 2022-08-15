using System.Net;
using RaceDirector.Remote.Networking;
using RaceDirector.Remote.Networking.Client;
using RaceDirector.Remote.Networking.Codec;
using Xunit;
using Xunit.Categories;

namespace Remote.Networking.Tests.Client;

[IntegrationTest]
public class SyncWsClientTest
{
    [Fact]
    public void RequiresProtocol()
    {
        var address = IPAddress.Loopback.ToString();

        Assert.Throws<UriFormatException>(() =>
        {
            new SyncWsClient<Nothing, Nothing>(address, ConstDecoder.Nothing.ToCodec(), TimeSpan.Zero);
        });
    }

    [Fact]
    public void AcceptsIpAddress()
    {
        var address = IPAddress.Loopback.ToString();

        var client = new SyncWsClient<Nothing, Nothing>($"ws://{address}", ConstDecoder.Nothing.ToCodec(), TimeSpan.Zero);

        Assert.Equal(address,client.Address);
        Assert.Equal(80, client.Port);
    }

    [Fact]
    public void AcceptsHostname()
    {
        var address = "localhost";

        var client =
            new SyncWsClient<Nothing, Nothing>($"ws://{address}", ConstDecoder.Nothing.ToCodec(), TimeSpan.Zero);

        Assert.Equal(IPAddress.Loopback.ToString(), client.Address);
        Assert.Equal(80, client.Port);
    }
}