using System;

namespace RaceDirector.Pipeline.Telemetry
{
    namespace V0
    {
        public interface ILiveTelemetry
        {
            /// <summary>
            /// Time since the physics simulation started.
            /// </summary>
            TimeSpan SimulationTime { get; }
        }
    }
}
