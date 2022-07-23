
using RaceDirector.Pipeline.Games;
using System;

namespace RaceDirector.Pipeline.Telemetry;

public interface ITelemetryObservableFactory : IGameInfo
{
    IObservable<V0.IGameTelemetry> CreateTelemetryObservable();
}