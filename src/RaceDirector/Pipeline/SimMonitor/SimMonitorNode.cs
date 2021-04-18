using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace RaceDirector.Pipeline.SimMonitor
{
    public class SimMonitorNode : IDisposable
    {
        public ISourceBlock<RunningSim> RunningSimSource
        {
            get;
        }

        public record Config(string[] GameNames, TimeSpan PollingInterval);

        public SimMonitorNode(Config config)
        {
            var ko = new KeepOne<string>(config.GameNames);
            RunningSimSource = ProcessPoller<RunningSim>(
                config.PollingInterval,
                processNames => ko.Call(processNames).Select(name => new RunningSim(name))
            );
        }

        public static ISourceBlock<T> ProcessPoller<T>(TimeSpan PollingInterval, Func<IEnumerable<string>, IEnumerable<T>> f)
        {
            var transformer = new TransformManyBlock<IEnumerable<string>, T>(f);

            var processPoller = new Timer(_ =>
            {
                var processes = Process.GetProcesses();
                var processNames = processes.Select(p => p.ProcessName);
                transformer.Post(processNames);
            }, null, TimeSpan.Zero, PollingInterval);

            transformer.Completion.ContinueWith(task => processPoller.Dispose());

            return transformer;
        }

        public void Dispose()
        {
            RunningSimSource.Complete();
        }
    }
}
