using System;

namespace RaceDirector.Pipeline.GameMonitor
{
    namespace V0
    {
        public interface IRunningGame {
            string? Name { get; }

            bool IsRunning()
            {
                return Name is not null;
            }
        };
    }
}
