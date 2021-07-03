using System;

namespace RaceDirector.Pipeline.Telemetry
{
    // TODO this is in the interface to make testing easier, but it might be abused
    public record GameTelemetry(TimeSpan SimulationTime) : V0.IGameTelemetry;
}
