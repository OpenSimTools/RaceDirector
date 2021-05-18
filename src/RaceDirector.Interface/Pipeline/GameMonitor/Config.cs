using System;

namespace RaceDirector.Pipeline.GameMonitor.Config
{
    namespace V0
    {
        public interface IProcessMonitorNodeConfig
        {
            TimeSpan PollingInterval { get; }
        }
    }

    // TODO remove when config done
    public record ProcessMonitorNodeConfig(TimeSpan PollingInterval) : V0.IProcessMonitorNodeConfig;
}
