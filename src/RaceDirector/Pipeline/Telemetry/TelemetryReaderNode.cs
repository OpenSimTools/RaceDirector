using RaceDirector.Pipeline.GameMonitor.V0;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;

namespace RaceDirector.Pipeline.Telemetry
{
    public class TelemetryReaderNode : INode, IDisposable
    {
        public ISourceBlock<V0.IGameTelemetry> GameTelemetrySource
        {
            get;
        }

        public ITargetBlock<IRunningGame> RunningGameTarget
        {
            get;
        }

        private ISourceBlock<V0.IGameTelemetry>? _currentGameTelemetrySource;

        public TelemetryReaderNode(IEnumerable<ITelemetrySourceFactory> telemetrySourceFactories)
        {
            var createSource = TelemetrySourceSelector(telemetrySourceFactories);
            var externalGameTelemetrySource = new BufferBlock<V0.IGameTelemetry>();
            GameTelemetrySource = externalGameTelemetrySource;
            RunningGameTarget = new ActionBlock<IRunningGame>(runningGame =>
            {
                _currentGameTelemetrySource?.Complete();
                _currentGameTelemetrySource = createSource(runningGame.Name);
                _currentGameTelemetrySource?.LinkTo(externalGameTelemetrySource, new DataflowLinkOptions());
            });
        }

        private Func<string?, ISourceBlock<V0.IGameTelemetry>?> TelemetrySourceSelector(IEnumerable<ITelemetrySourceFactory> telemetrySourceFactories)
        {
            return gameName =>
                telemetrySourceFactories
                    .Where(tsf => tsf.GameName.Equals(gameName))
                    .Select(tsf => tsf.CreateTelemetrySource())
                    .FirstOrDefault();
        }

        public void Dispose()
        {
            GameTelemetrySource.Complete();
            RunningGameTarget.Complete();
        }
    }
}
