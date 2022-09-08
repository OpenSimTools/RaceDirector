using Microsoft.Reactive.Testing;
using RaceDirector.DeviceIO.Pipeline;
using RaceDirector.PitCrew.Pipeline;
using RaceDirector.PitCrew.Protocol;
using Xunit;
using Xunit.Categories;

namespace PitCrew.Plugin.Tests.Pipeline;

[UnitTest]
public class PitMenuNodeTest : ReactiveTest
{
    [Fact]
    public void AlwaysAddWhatIsRequestedInsteadOfChangingTheMenu()
    {
        var testScheduler = new TestScheduler();
        var input = testScheduler.CreateColdObservable(
            OnNext<IPitStrategyRequest>(0, new PitStrategyRequest(3)),
            OnCompleted<IPitStrategyRequest>(10)
        );
        var output = testScheduler.CreateObserver<GameAction>();

        var node = new PitMenuNode();
        input.Subscribe(node.PitStrategyObserver);
        node.GameActionObservable.Subscribe(output);

        testScheduler.Start();

        output.Messages.AssertEqual(
            OnNext(1, GameAction.PitMenuOpen), // TODO why 1?!
            OnNext(1, GameAction.PitMenuDown),
            OnNext(1, GameAction.PitMenuDown),
            OnNext(1, GameAction.PitMenuRight),
            OnNext(1, GameAction.PitMenuRight),
            OnNext(1, GameAction.PitMenuRight),
            OnCompleted<GameAction>(10)
        );
    }
}