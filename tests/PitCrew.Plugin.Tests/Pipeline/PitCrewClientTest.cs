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
    private static readonly TimeSpan Timeout = TimeSpan.FromMilliseconds(1000);

    [Fact]
    public void ReceivesPitStrategyFromClients()
    {
        WithServerClient((testServer, pitCrewClient) =>
        {
            var testScheduler = new TestScheduler();
            var testObserver = testScheduler.CreateObserver<IPitStrategyRequest?>();
            pitCrewClient.In.Subscribe(testObserver);

            testServer.WsMulticastAsync(
                @"{
                  ""PitStrategyRequest"": {
                    ""FuelToAddL"": 1,
                    ""TireSet"": 2,
                    ""FrontTires"": {
                      ""LeftPressureKpa"": 3.1,
                      ""RightPressureKpa"": 3.2
                    },
                    ""RearTires"": {
                      ""LeftPressureKpa"": 4.1,
                      ""RightPressureKpa"": 4.2
                    }
                  }
                }"
            );

            EventuallyAssertion.Eventually(() =>
                    Assert.Equal(
                        new [] { new PitMenu
                        (
                            FuelToAddL: 1,
                            TireSet: 2,
                            FrontTires: new PitMenuTires(3.1, 3.2),
                            RearTires: new PitMenuTires(4.1, 4.2)
                        )},
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

            testServer.WsMulticastAsync("{\"PitStrategyRequest\": {\"FuelToAddL\":true}}");

            EventuallyAssertion.Eventually(() =>
                    Assert.Equal(
                        new PitMenu?[] { null },
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

        using var pitCrewClient = new PitCrewClient($"ws://{IPAddress.Loopback}:{serverPort}", TimeSpan.Zero);
        Assert.True(pitCrewClient.Connect());
        pitCrewClient.Connected.Wait(Timeout);

        action(testServer, pitCrewClient);
    }
}