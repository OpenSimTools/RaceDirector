using RaceDirector.Pipeline.GameMonitor.V0;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using RaceDirector.Pipeline.Telemetry.V0;
using RaceDirector.Reactive;

namespace RaceDirector.Pipeline.Telemetry;

public sealed class TelemetryReaderNode : INode, IDisposable
{
    public IObservable<IGameTelemetry> GameTelemetryObservable => _subject.SelectManyUntilNext(rg => _createObservable(rg));

    public IObserver<IRunningGame> RunningGameObserver => _subject;

    private readonly Subject<IRunningGame> _subject;
    private readonly Func<IRunningGame, IObservable<IGameTelemetry>> _createObservable;

    public TelemetryReaderNode(IEnumerable<ITelemetryObservableFactory> telemetryObservableFactories)
    {
        _createObservable = TelemetryObservableSelector(telemetryObservableFactories);
        _subject = new Subject<IRunningGame>();
    }

    private Func<IRunningGame, IObservable<IGameTelemetry>> TelemetryObservableSelector(
        IEnumerable<ITelemetryObservableFactory> telemetryObservableFactories)
    {
        return runningGame => Observable.Return(GameTelemetry.Empty)
            .Concat(telemetryObservableFactories
                .FirstOrDefault(tsf => tsf.GameName.Equals(runningGame.Name))
                ?.CreateTelemetryObservable()
            ?? Observable.Empty<IGameTelemetry>());
    }

    public void Dispose()
    {
        _subject.Dispose();
    }
}