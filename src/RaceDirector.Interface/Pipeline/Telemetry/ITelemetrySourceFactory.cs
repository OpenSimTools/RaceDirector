
using RaceDirector.Pipeline.Games;
using System;

namespace RaceDirector.Pipeline.Telemetry
{
    public interface ITelemetrySourceFactory : IGameInfo
    {
        IObservable<V0.IGameTelemetry> CreateTelemetrySource();
    }
}
