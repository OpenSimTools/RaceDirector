using Microsoft.Reactive.Testing;
using Moq;
using RaceDirector.DeviceIO.Pipeline;
using RaceDirector.Pipeline.Telemetry.V0;
using RaceDirector.PitCrew.Pipeline;
using RaceDirector.PitCrew.Pipeline.Games;
using RaceDirector.PitCrew.Protocol;
using Xunit;
using Xunit.Categories;

namespace PitCrew.Plugin.Tests.Pipeline;

[UnitTest]
public class PitMenuNodeTest : ReactiveTest
{
    [Fact]
    public void DelegatesPitMenuNavigation()
    {
        var testScheduler = new TestScheduler();
        var psr = new PitStrategyRequest(42);

        var pitStrategyObservable = testScheduler.CreateColdObservable(
            OnNext<IPitStrategyRequest>(3, psr),
            OnCompleted<IPitStrategyRequest>(5)
        );

        var gameActionObserver = testScheduler.CreateObserver<GameAction>();

        var pitMenuNavigatorMock = new Mock<IGamePitMenuNavigator>();
        pitMenuNavigatorMock
            .Setup(_ => _.SetStrategy(psr, It.IsAny<IObservable<IGameTelemetry>>()))
            .Returns(testScheduler.CreateColdObservable(
                OnNext(7, GameAction.PitMenuUp),
                OnNext(7, GameAction.PitMenuDown),
                OnCompleted<GameAction>(11)
            ));

        var node = new PitMenuNode(pitMenuNavigatorMock.Object);

        node.GameActionObservable.Subscribe(gameActionObserver);
        pitStrategyObservable.Subscribe(node.PitStrategyObserver);
        testScheduler.Start();

        gameActionObserver.Messages.AssertEqual(
            OnNext(3+7, GameAction.PitMenuUp),
            OnNext(3+7, GameAction.PitMenuDown),
            OnCompleted<GameAction>(3+11)
        );
    }
}