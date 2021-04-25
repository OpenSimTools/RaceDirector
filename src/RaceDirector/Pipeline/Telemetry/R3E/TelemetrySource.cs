using RaceDirector.Pipeline.Utils;
using System;
using System.Runtime.Versioning;
using System.Threading.Tasks.Dataflow;

namespace RaceDirector.Pipeline.Telemetry.R3E
{
    [SupportedOSPlatform("windows")]
    class TelemetrySource
    {
        private Config _config;

        public record Config(TimeSpan PollingInterval);

        public TelemetrySource(Config config)
        {
            _config = config;
        }

        public ISourceBlock<LiveTelemetry> Create()
        {
            var mmReader = new MemoryMappedFileReader<Contrib.Data.Shared>(Contrib.Constant.SharedMemoryName);
            return PollingSource.Create<LiveTelemetry>(_config.PollingInterval, () => Transform(mmReader.Read()));
        }

        private LiveTelemetry Transform(Contrib.Data.Shared sharedData)
        {
            return new LiveTelemetry(TimeSpan.FromSeconds(sharedData.Player.GameSimulationTime));
        }
    }
}
