using RaceDirector.Pipeline.GameMonitor.V0;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using RaceDirector.Pipeline.Utils;

namespace RaceDirector.Pipeline.Telemetry;

public sealed class TelemetryReaderNode : INode, IDisposable
{
    public IObservable<V0.IGameTelemetry> GameTelemetryObservable => _subject.SelectManyUntilNext(rg => _createObservable(rg));

    public IObserver<IRunningGame> RunningGameObserver => _subject;

    private readonly Subject<IRunningGame> _subject;
    private Func<IRunningGame, IObservable<V0.IGameTelemetry>> _createObservable;

    public TelemetryReaderNode(IEnumerable<ITelemetryObservableFactory> telemetryObservableFactories)
    {
        _createObservable = telemetryObservableSelector(telemetryObservableFactories);
        _subject = new Subject<IRunningGame>();
    }

    private Func<IRunningGame, IObservable<V0.IGameTelemetry>> telemetryObservableSelector(IEnumerable<ITelemetryObservableFactory> telemetryObservableFactories)
    {
        return runningGame =>
            telemetryObservableFactories
                .FirstOrDefault(tsf => tsf.GameName.Equals(runningGame.Name))
                ?.CreateTelemetryObservable()
            ?? Observable.Empty<V0.IGameTelemetry>();
    }

    public void Dispose()
    {
        _subject.Dispose();
    }
}