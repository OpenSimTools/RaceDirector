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
using static TestUtils.EventuallyAssertion;

namespace PitCrew.Plugin.Tests.Pipeline;

[IntegrationTest]
public class PitCrewNodeTest
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMilliseconds(500);

    [Fact]
    public void ReceivesPitStrategyFromClients()
    {
        WithNodeServer((node, server) =>
        {
            var testScheduler = new TestScheduler();
            var testObserver = testScheduler.CreateObserver<IPitStrategyRequest>();
            node.PitStrategyObservable.Subscribe(testObserver);

            server.WsMulticastAsync("{\"PitStrategyRequest\": {\"FuelToAddL\":2}}");

            Eventually(() =>
                    Assert.Equal(
                        new [] { new PitMenu
                        (
                            FuelToAddL: 2,
                            TireSet: null,
                            FrontTires: null,
                            RearTires: null
                        )},
                        testObserver.ReceivedValues()
                    )
                )
                .Within(Timeout);
        });
    }
    
    private static void WithNodeServer(Action<PitCrewNode, IWsServer<string, string>> action)
    {
        var serverPort = Tcp.FreePort();

        using var testServer = new MultiEndpointWsServer<string, string>(IPAddress.Loopback, serverPort, new[]
        {
            new HttpEndpoint<string, string>(_ => true, Codec.UTF8String)
        }, NullLogger.Instance);
        Assert.True(testServer.Start());

        using var pitCrewClient = new PitCrewClient($"ws://{IPAddress.Loopback}:{serverPort}", TimeSpan.Zero);
        var node = new PitCrewNode(pitCrewClient);
        // This would be done by the Remote.Networking plugin
        Assert.True(pitCrewClient.Connect());
        pitCrewClient.Connected.Wait(Timeout);

        action(node, testServer);
    }
}