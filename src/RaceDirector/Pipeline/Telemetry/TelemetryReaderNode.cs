using RaceDirector.Pipeline.GameMonitor.V0;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using RaceDirector.Pipeline.Utils;

namespace RaceDirector.Pipeline.Telemetry
{
    public class TelemetryReaderNode : INode, IDisposable
    {
        public IObservable<V0.IGameTelemetry> GameTelemetrySource
        {
            get => _subject.SelectManyUntilNext(rg => _createSource(rg));
        }

        public IObserver<IRunningGame> RunningGameTarget
        {
            get => _subject;
        }

        private Subject<IRunningGame> _subject;
        private Func<IRunningGame, IObservable<V0.IGameTelemetry>> _createSource;

        public TelemetryReaderNode(IEnumerable<ITelemetrySourceFactory> telemetrySourceFactories)
        {
            _createSource = telemetrySourceSelector(telemetrySourceFactories);
            _subject = new Subject<IRunningGame>();
        }

        private Func<IRunningGame, IObservable<V0.IGameTelemetry>> telemetrySourceSelector(IEnumerable<ITelemetrySourceFactory> telemetrySourceFactories)
        {
            return runningGame =>
                telemetrySourceFactories
                    .FirstOrDefault(tsf => tsf.GameName.Equals(runningGame.Name))
                    ?.CreateTelemetrySource()
                    ?? Observable.Empty<V0.IGameTelemetry>();
        }

        public void Dispose()
        {
            _subject.Dispose();
        }
    }
}
