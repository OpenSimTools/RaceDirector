using RaceDirector.Pipeline.Utils;
using System;
using System.Runtime.Versioning;
using System.Threading.Tasks.Dataflow;

namespace RaceDirector.Pipeline.Games.R3E
{
    [SupportedOSPlatform("windows")]
    public class Game : IGame
    {
        public record Config(TimeSpan PollingInterval); // TODO remove when config done

        private Config _config;

        public string GameName => "R3E";

        public string[] GameProcessNames => new[] { "RRRE64", "RRRE" };

        public Game(Config config)
        {
            _config = config;
        }

        public ISourceBlock<Pipeline.Telemetry.V0.IGameTelemetry> CreateTelemetrySource()
        {
            var mmReader = new MemoryMappedFileReader<Contrib.Data.Shared>(Contrib.Constant.SharedMemoryName);
            var telemetry = new Telemetry();
            return PollingSource.Create<Pipeline.Telemetry.V0.IGameTelemetry>(_config.PollingInterval, () => telemetry.Transform(mmReader.Read()));
        }
    }
}
