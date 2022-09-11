using AutoBogus;
using AutoBogus.Moq;
using Microsoft.Reactive.Testing;
using RaceDirector.Pipeline.GameMonitor;
using RaceDirector.Pipeline.Telemetry;
using RaceDirector.Pipeline.Telemetry.V0;
using System;
using System.Reactive.Linq;
using Xunit;
using Xunit.Categories;
using static System.Array;

namespace RaceDirector.Tests.Pipeline.Telemetry;

[UnitTest]
public class TelemetryReaderNodeTest : ReactiveTest
{
    private static readonly IGameTelemetry[] Telemetry = AutoFaker.Generate<IGameTelemetry>(3, b => b.WithBinder<MoqBinder>()).ToArray();

    private readonly TestScheduler _testScheduler;
    private readonly ITestableObserver<IGameTelemetry> _testObserver;

    public TelemetryReaderNodeTest()
    {
        _testScheduler = new TestScheduler();
        _testObserver = _testScheduler.CreateObserver<IGameTelemetry>();
    }

    [Fact]
    public void DoesNotEmitWhenGameNotMatching()
    {
        var trn = new TelemetryReaderNode(Empty<ITelemetryObservableFactory>());
        trn.GameTelemetryObservable.Subscribe(_testObserver);

        _testScheduler.CreateColdObservable(
            OnNext(3, new RunningGame(null)),
            OnNext(5, new RunningGame("any")),
            OnCompleted<RunningGame>(7)
        ).Subscribe(trn.RunningGameObserver);

        _testScheduler.Start();

        _testObserver.Messages.AssertEqual(
            OnNext<IGameTelemetry>(3, GameTelemetry.Empty),
            OnNext<IGameTelemetry>(5, GameTelemetry.Empty),
            OnCompleted<IGameTelemetry>(7)
        );
    }

    [Fact]
    public void SwitchesSourcesWhenGameChanges()
    {
        var trn = new TelemetryReaderNode(new ITelemetryObservableFactory[]
        {
            new TestTelemetryObservableFactory("a", Telemetry[0]),
            new TestTelemetryObservableFactory("b", Telemetry[1], Telemetry[2])
        });
        trn.GameTelemetryObservable.Subscribe(_testObserver);
        
        _testScheduler.CreateColdObservable(
            OnNext(3, new RunningGame("a")),
            OnNext(5, new RunningGame("b")),
            OnNext(7, new RunningGame("a")),
            OnCompleted<RunningGame>(9)
        ).Subscribe(trn.RunningGameObserver);

        _testScheduler.Start();

        _testObserver.Messages.AssertEqual(
            OnNext<IGameTelemetry>(3, GameTelemetry.Empty),
            OnNext(3, Telemetry[0]),
            OnNext<IGameTelemetry>(5, GameTelemetry.Empty),
            OnNext(5, Telemetry[1]),
            OnNext(5, Telemetry[2]),
            OnNext<IGameTelemetry>(7, GameTelemetry.Empty),
            OnNext(7, Telemetry[0]),
            OnCompleted<IGameTelemetry>(9)
        );
    }

    private record TestTelemetryObservableFactory(string GameName, params IGameTelemetry[] Elements) : ITelemetryObservableFactory
    {
        public IObservable<IGameTelemetry> CreateTelemetryObservable()
        {
            return Elements.ToObservable();
        }
    }
}