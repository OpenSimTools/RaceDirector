﻿using RaceDirector.Pipeline.Utils;
using System;
using System.Reactive.Linq;
using System.Runtime.Versioning;
using RaceDirector.Pipeline.Telemetry.V0;

namespace RaceDirector.Pipeline.Games.ACC
{
    [SupportedOSPlatform("windows")]
    public class Game : IGame
    {
        public record Config(TimeSpan PollingInterval); // TODO remove when config done

        private readonly Config _config;

        public string GameName => "ACC";

        public string[] GameProcessNames => new[] { "AC2-Win64-Shipping" };

        public Game(Config config)
        {
            _config = config;
        }

        public IObservable<IGameTelemetry> CreateTelemetryObservable()
        {
            var physicsMmReader = new MemoryMappedFileReader<Contrib.Data.SPageFilePhysics>(Contrib.Constant.SharedMemoryPhysicsName);
            var graphicMmReader = new MemoryMappedFileReader<Contrib.Data.SPageFileGraphic>(Contrib.Constant.SharedMemoryGraphicName);
            var staticMmReader = new MemoryMappedFileReader<Contrib.Data.SPageFileStatic>(Contrib.Constant.SharedMemoryStaticName);
            var telemetryConverter = new TelemetryConverter();
            return Observable.Interval(_config.PollingInterval)
                .SelectMany(_ =>
                {
                    try
                    {
                        Contrib.Data.Shared shared;
                        shared.Physics = physicsMmReader.Read();
                        shared.Graphic = graphicMmReader.Read();
                        shared.Static = staticMmReader.Read();
                        var telemetry = telemetryConverter.Transform(ref shared);
                        return Observable.Return(telemetry);
                    }
                    catch
                    {
                        return Observable.Empty<IGameTelemetry>();
                    }
                });
        }


    }
}
