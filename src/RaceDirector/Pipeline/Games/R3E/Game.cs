using RaceDirector.Pipeline.Utils;
using System;
using System.Reactive.Linq;
using System.Runtime.Versioning;

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

        public IObservable<Pipeline.Telemetry.V0.IGameTelemetry> CreateTelemetryObservable()
        {
            var mmReader = new MemoryMappedFileReader<Contrib.Data.Shared>(Contrib.Constant.SharedMemoryName);
            var telemetryConverter = new TelemetryConverter();
            return Observable.Interval(_config.PollingInterval)
                .SelectMany(_ =>
                {
                    try
                    {
                        var shared = mmReader.Read();
                        var telemetry = telemetryConverter.Transform(shared);
                        return Observable.Return(telemetry);
                    }
                    catch
                    {
                        return Observable.Empty<Pipeline.Telemetry.GameTelemetry>();
                    }
                });
        }
    }
}
