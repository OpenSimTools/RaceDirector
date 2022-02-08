using RaceDirector.Pipeline.GameMonitor.V0;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace RaceDirector.Pipeline.Telemetry
{
    public class TelemetryReaderNode : INode, IDisposable
    {
        public IObservable<V0.IGameTelemetry> GameTelemetrySource
        {
            get => _subject.SelectMany(rg => _createSource(rg) ?? Observable.Empty<V0.IGameTelemetry>());
        }

        public IObserver<IRunningGame> RunningGameTarget
        {
            get => _subject;
        }

        private Subject<IRunningGame> _subject;
        private Func<IRunningGame, IObservable<V0.IGameTelemetry>?> _createSource;

        public TelemetryReaderNode(IEnumerable<ITelemetrySourceFactory> telemetrySourceFactories)
        {
            _createSource = telemetrySourceSelector(telemetrySourceFactories);
            _subject = new Subject<IRunningGame>();
        }

        private Func<IRunningGame, IObservable<V0.IGameTelemetry>?> telemetrySourceSelector(IEnumerable<ITelemetrySourceFactory> telemetrySourceFactories)
        {
            return runningGame =>
                telemetrySourceFactories
                    .Where(tsf => tsf.GameName.Equals(runningGame.Name))
                    .Select(tsf => tsf.CreateTelemetrySource())
                    .FirstOrDefault();
        }

        public void Dispose()
        {
            _subject.Dispose();
        }
    }
}
