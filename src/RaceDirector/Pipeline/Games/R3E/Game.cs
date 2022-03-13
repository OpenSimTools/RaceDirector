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

        public IObservable<Pipeline.Telemetry.V0.IGameTelemetry> CreateTelemetrySource()
        {
            var mmReader = new MemoryMappedFileReader<Contrib.Data.Shared>(Contrib.Constant.SharedMemoryName);
            var telemetry = new Telemetry();
            return Observable.Interval(_config.PollingInterval)
                .SelectMany(_ =>
                {
                    try
                    {
                        Console.Write("x");
                        return Observable.Return(telemetry.Transform(mmReader.Read()));
                    }
                    catch
                    {
                        Console.WriteLine("@");
                        return Observable.Empty<Pipeline.Telemetry.GameTelemetry>();
                    }
                });
        }
    }
}
