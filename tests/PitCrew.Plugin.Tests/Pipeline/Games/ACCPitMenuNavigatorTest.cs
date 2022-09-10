using Microsoft.Reactive.Testing;
using RaceDirector.DeviceIO.Pipeline;
using RaceDirector.Pipeline.Telemetry.V0;
using RaceDirector.PitCrew.Pipeline.Games;
using RaceDirector.PitCrew.Protocol;
using Xunit;
using Xunit.Categories;

namespace PitCrew.Plugin.Tests.Pipeline.Games;

[UnitTest]
public class ACCPitMenuNavigatorTest : ReactiveTest
{
    [Fact]
    public void AlwaysAddWhatIsRequestedInsteadOfChangingTheMenu()
    {
        var testScheduler = new TestScheduler();
        var gameTelemetryObservable = testScheduler.CreateColdObservable(
            OnCompleted<IGameTelemetry>(10)
        );

        var pmn = new ACCPitMenuNavigator();
        var psr = new PitStrategyRequest(3);

        var output = testScheduler.Start(() => pmn.SetStrategy(psr, gameTelemetryObservable));

        output.Messages.AssertEqual(
            OnNext(200, GameAction.PitMenuOpen),
            OnNext(200, GameAction.PitMenuDown),
            OnNext(200, GameAction.PitMenuDown),
            OnNext(200, GameAction.PitMenuRight),
            OnNext(200, GameAction.PitMenuRight),
            OnNext(200, GameAction.PitMenuRight),
            OnCompleted<GameAction>(200)
        );
    }
}