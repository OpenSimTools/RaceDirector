
using System;
using System.Threading.Tasks.Dataflow;

namespace RaceDirector.Pipeline
{
    /// <summary>
    /// Temporary node to demo capability to use multiple targets for telemetry.
    /// </summary>
    public class TelemetryLoggerNode : INode, IDisposable
    {
        public ITargetBlock<Telemetry.V0.ILiveTelemetry> LiveTelemetryTarget =>
            new ActionBlock<Telemetry.V0.ILiveTelemetry>(liveTelemetry =>
                Console.WriteLine("> " + liveTelemetry.SimulationTime.TotalSeconds)
            );

        public void Dispose()
        {
            LiveTelemetryTarget.Complete();
        }
    }
}
