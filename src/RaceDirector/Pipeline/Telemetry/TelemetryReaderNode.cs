using RaceDirector.Pipeline.GameMonitor.V0;
using System;
using System.Runtime.Versioning;
using System.Threading.Tasks.Dataflow;

namespace RaceDirector.Pipeline.Telemetry
{
    public class TelemetryReaderNode : IDisposable
    {
        public ISourceBlock<LiveTelemetry> LiveTelemetrySource
        {
            get;
        }

        public ITargetBlock<IRunningGame> RunningGameTarget
        {
            get;
        }

        private ISourceBlock<LiveTelemetry>? _currentLiveTelemetrySource;

        [SupportedOSPlatform("windows")]
        public TelemetryReaderNode() :
            this(_ => new R3E.TelemetrySource(new R3E.TelemetrySource.Config(TimeSpan.FromMilliseconds(500))).Create()) { }

        public TelemetryReaderNode(Func<string?, ISourceBlock<LiveTelemetry>?> telemetrySourceSelector)
        {
            var outsideLiveTelemetrySource = new BufferBlock<LiveTelemetry>();
            LiveTelemetrySource = outsideLiveTelemetrySource;
            RunningGameTarget = new ActionBlock<IRunningGame>(runningGame =>
            {
                _currentLiveTelemetrySource?.Complete();
                _currentLiveTelemetrySource = telemetrySourceSelector(runningGame.Name);
                _currentLiveTelemetrySource?.LinkTo(outsideLiveTelemetrySource, new DataflowLinkOptions());
            });
        }

        public void Dispose()
        {
            LiveTelemetrySource.Complete();
            RunningGameTarget.Complete();
        }
    }
}
