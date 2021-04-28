using System;

namespace RaceDirector.Pipeline.Telemetry
{
    public record LiveTelemetry(TimeSpan SimulationTime) : V0.ILiveTelemetry;
}
