using System;

namespace RaceDirector.Pipeline.Telemetry
{
    // TODO this is in the interface to make testing easier, but it might be abused
    public record LiveTelemetry(TimeSpan SimulationTime) : V0.ILiveTelemetry;
}
