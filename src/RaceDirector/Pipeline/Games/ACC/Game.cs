using RaceDirector.Pipeline.Telemetry;
using RaceDirector.Pipeline.Utils;
using System;
using System.IO;
using System.Reactive.Linq;
using System.Runtime.Versioning;

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

        public IObservable<Telemetry.V0.IGameTelemetry> CreateTelemetryObservable()
        {
            var physicsMmReader = new MemoryMappedFileReader<Contrib.Data.SPageFilePhysics>(Contrib.Constant.SharedMemoryPhysicsName);
            var graphicMmReader = new MemoryMappedFileReader<Contrib.Data.SPageFileGraphic>(Contrib.Constant.SharedMemoryGraphicName);
            var staticMmReader = new MemoryMappedFileReader<Contrib.Data.SPageFileStatic>(Contrib.Constant.SharedMemoryStaticName);
            var telemetryConverter = new TelemetryConverter();
            return Observable.Interval(_config.PollingInterval)
                .Select(_ =>
                {
                    try
                    {
                        Contrib.Data.Shared shared;
                        shared.Physics = physicsMmReader.Read();
                        shared.Graphic = graphicMmReader.Read();
                        shared.Static = staticMmReader.Read();
                        var telemetry = telemetryConverter.Transform(ref shared);
                        return telemetry;
                    }
                    catch (FileNotFoundException)
                    {
                        return NoMMFiles;
                    }
                });
        }

        private static GameTelemetry NoMMFiles = new(
            GameState: Telemetry.V0.GameState.Menu,
            UsingVR: null,
            Event: null,
            Session: null,
            Vehicles: Array.Empty<Vehicle>(),
            FocusedVehicle: null,
            Player: null
        );
    }
}
