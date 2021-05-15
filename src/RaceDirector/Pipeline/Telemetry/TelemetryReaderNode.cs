using RaceDirector.Pipeline.GameMonitor.V0;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;

namespace RaceDirector.Pipeline.Telemetry
{
    public class TelemetryReaderNode : INode, IDisposable
    {
        public ISourceBlock<V0.ILiveTelemetry> LiveTelemetrySource
        {
            get;
        }

        public ITargetBlock<IRunningGame> RunningGameTarget
        {
            get;
        }

        private ISourceBlock<V0.ILiveTelemetry>? _currentLiveTelemetrySource;

        public TelemetryReaderNode(IEnumerable<ITelemetrySourceFactory> telemetrySourceFactories)
        {
            var createSource = TelemetrySourceSelector(telemetrySourceFactories);
            var outsideLiveTelemetrySource = new BufferBlock<V0.ILiveTelemetry>();
            LiveTelemetrySource = outsideLiveTelemetrySource;
            RunningGameTarget = new ActionBlock<IRunningGame>(runningGame =>
            {
                _currentLiveTelemetrySource?.Complete();
                _currentLiveTelemetrySource = createSource(runningGame.Name);
                _currentLiveTelemetrySource?.LinkTo(outsideLiveTelemetrySource, new DataflowLinkOptions());
            });
        }

        private Func<string?, ISourceBlock<V0.ILiveTelemetry>?> TelemetrySourceSelector(IEnumerable<ITelemetrySourceFactory> telemetrySourceFactories)
        {
            return (string? gameName) =>
                telemetrySourceFactories
                    .Where(tsf => tsf.GameName.Equals(gameName))
                    .Select(tsf => tsf.CreateTelemetrySource())
                    .FirstOrDefault();
        }

        public void Dispose()
        {
            LiveTelemetrySource.Complete();
            RunningGameTarget.Complete();
        }
    }
}
