using System;
using System.Runtime.Versioning;
using System.Threading.Tasks.Dataflow;
using RaceDirector.Pipeline.Telemetry.V0;

namespace RaceDirector.Pipeline.Games.ACC
{
    [SupportedOSPlatform("windows")]
    public class Game : IGame
    {
        public record Config(TimeSpan PollingInterval); // TODO remove when config done

        private Config _config;

        public string GameName => "ACC";

        public string[] GameProcessNames => new[] { "AC2-Win64-Shipping" };

        public Game(Config config)
        {
            _config = config;
        }

        public IObservable<IGameTelemetry> CreateTelemetryObservable()
        {
            throw new NotImplementedException();
        }
    }
}
