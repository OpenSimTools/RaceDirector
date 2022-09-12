using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Reactive.Testing;
using Moq;
using RaceDirector.DeviceIO.Pipeline;
using RaceDirector.Pipeline.GameMonitor;
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
    private readonly TestScheduler _testScheduler;
    private readonly string _testGameName;
    private readonly IPitStrategyRequest _pitStrategyRequest;

    public PitMenuNodeTest()
    {
        _testScheduler = new TestScheduler();
        _testGameName = "GAM";
        _pitStrategyRequest = new Mock<IPitStrategyRequest>().Object;
    }

    [Fact]
    public void DefaultsToNoopWhenGameNotStarted()
    {
        var pitStrategyObservable = _testScheduler.CreateColdObservable(
            OnNext(3, _pitStrategyRequest),
            OnCompleted<IPitStrategyRequest>(5)
        );

        var gameActionObserver = _testScheduler.CreateObserver<GameAction>();

        var pitMenuNavigatorMock = new Mock<IGamePitMenuNavigator>();
        pitMenuNavigatorMock
            .SetupGet(_ => _.GameName).Returns(_testGameName);

        var node = new PitMenuNode(new [] { pitMenuNavigatorMock.Object }, NullLogger<PitMenuNode>.Instance);

        node.GameActionObservable.Subscribe(gameActionObserver);
        pitStrategyObservable.Subscribe(node.PitStrategyObserver);
        Observable.Return(new RunningGame(null)).Subscribe(node.RunningGameObserver);
        
        _testScheduler.Start();

        gameActionObserver.Messages.AssertEqual(
            OnCompleted<GameAction>(5)
        );
    }
    
    [Fact]
    public void DefaultsToNoopWhenGameNotMatches()
    {
        var pitStrategyObservable = _testScheduler.CreateColdObservable(
            OnNext(3, _pitStrategyRequest),
            OnCompleted<IPitStrategyRequest>(5)
        );

        var gameActionObserver = _testScheduler.CreateObserver<GameAction>();

        var pitMenuNavigatorMock = new Mock<IGamePitMenuNavigator>();
        pitMenuNavigatorMock
            .SetupGet(_ => _.GameName).Returns($"NOT{_testGameName}");

        var node = new PitMenuNode(new[] {pitMenuNavigatorMock.Object}, NullLogger<PitMenuNode>.Instance);

        node.GameActionObservable.Subscribe(gameActionObserver);
        pitStrategyObservable.Subscribe(node.PitStrategyObserver);
        Observable.Return(new RunningGame(_testGameName)).Subscribe(node.RunningGameObserver);
        
        _testScheduler.Start();

        gameActionObserver.Messages.AssertEqual(
            OnCompleted<GameAction>(5)
        );
    }

    [Fact]
    public void DelegatesWhenGameMatches()
    {
        var psr = new Mock<IPitStrategyRequest>().Object;

        var pitStrategyObservable = _testScheduler.CreateColdObservable(
            OnNext(3, psr),
            OnCompleted<IPitStrategyRequest>(5)
        );

        var gameActionObserver = _testScheduler.CreateObserver<GameAction>();

        var pitMenuNavigatorMock = new Mock<IGamePitMenuNavigator>();
        pitMenuNavigatorMock
            .SetupGet(_ => _.GameName).Returns(_testGameName);
        pitMenuNavigatorMock
            .Setup(_ => _.SetStrategy(psr,
                It.IsAny<IObservable<IGameTelemetry>>(), It.IsAny<ILogger>()))
            .Returns(_testScheduler.CreateColdObservable(
                OnNext(7, GameAction.PitMenuUp),
                OnNext(7, GameAction.PitMenuDown),
                OnCompleted<GameAction>(11)
            ));

        var node = new PitMenuNode(new [] { pitMenuNavigatorMock.Object }, NullLogger<PitMenuNode>.Instance);

        node.GameActionObservable.Subscribe(gameActionObserver);
        pitStrategyObservable.Subscribe(node.PitStrategyObserver);
        Observable.Return(new RunningGame(_testGameName)).Subscribe(node.RunningGameObserver);

        _testScheduler.Start();

        gameActionObserver.Messages.AssertEqual(
            OnNext(3+7, GameAction.PitMenuUp),
            OnNext(3+7, GameAction.PitMenuDown),
            OnCompleted<GameAction>(3+11)
        );
    }
}