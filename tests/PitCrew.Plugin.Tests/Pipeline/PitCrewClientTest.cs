using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Reactive.Testing;
using RaceDirector.PitCrew.Pipeline;
using RaceDirector.PitCrew.Protocol;
using RaceDirector.Remote.Networking;
using RaceDirector.Remote.Networking.Server;
using TestUtils;
using Xunit;
using Xunit.Categories;

namespace PitCrew.Plugin.Tests.Pipeline;

[IntegrationTest]
public class PitCrewClientTest
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMilliseconds(500);

    [Fact]
    public void ReceivesPitStrategyFromClients()
    {
        WithServerClient((testServer, pitCrewClient) =>
        {
            var testScheduler = new TestScheduler();
            var testObserver = testScheduler.CreateObserver<IPitStrategyRequest?>();
            pitCrewClient.In.Subscribe(testObserver);

            testServer.WsMulticastAsync("{\"PitStrategyRequest\": {\"FuelToAdd\":2}}");

            EventuallyAssertion.Eventually(() =>
                    Assert.Equal(
                        new [] { new PitStrategyRequest(2) },
                        testObserver.ReceivedValues()
                    )
                )
                .Within(Timeout);
        });
    }
    
    [Fact]
    public void InvalidPitStrategiesBecomeNull()
    {
        WithServerClient((testServer, pitCrewClient) =>
        {
            var testScheduler = new TestScheduler();
            var testObserver = testScheduler.CreateObserver<IPitStrategyRequest?>();
            pitCrewClient.In.Subscribe(testObserver);

            testServer.WsMulticastAsync("{\"PitStrategyRequest\": {\"FuelToAdd\":true}}");

            EventuallyAssertion.Eventually(() =>
                    Assert.Equal(
                        new PitStrategyRequest?[] { null },
                        testObserver.ReceivedValues()
                    )
                )
                .Within(Timeout);
        });
    }
    
    // TODO check all codec edge cases
    
    private static void WithServerClient(Action<IWsServer<string, string>, PitCrewClient> action)
    {
        var serverPort = Tcp.FreePort();

        using var testServer = new MultiEndpointWsServer<string, string>(IPAddress.Loopback, serverPort, new[]
        {
            new HttpEndpoint<string, string>(_ => true, Codec.UTF8String)
        }, NullLogger.Instance);
        Assert.True(testServer.Start());

        using var pitCrewClient = new PitCrewClient($"ws://{IPAddress.Loopback}:{serverPort}");
        Assert.True(pitCrewClient.Connect());
        pitCrewClient.Connected.Wait(Timeout);

        action(testServer, pitCrewClient);
    }
}