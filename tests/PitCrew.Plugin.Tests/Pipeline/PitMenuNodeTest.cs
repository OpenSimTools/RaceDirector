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
    public void TransformsAnyPitStrategyRequestIntoPitMenuOpenAction()
    {
        var testScheduler = new TestScheduler();
        var input = testScheduler.CreateColdObservable(
            OnNext<IPitStrategyRequest>(0, new PitStrategyRequest(null)),
            OnCompleted<IPitStrategyRequest>(10)
        );
        var output = testScheduler.CreateObserver<GameAction>();

        var node = new PitMenuNode();
        input.Subscribe(node.PitStrategyObserver);
        node.GameActionObservable.Subscribe(output);

        testScheduler.Start();

        output.Messages.AssertEqual(
            OnNext(1, GameAction.PitMenuOpen), // TODO why 1?!
            OnCompleted<GameAction>(10)
        );
    }
}