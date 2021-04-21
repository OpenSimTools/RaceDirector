using RaceDirector.Pipeline.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        private ISourceBlock<T> ProcessPoller<T>(TimeSpan pollingInterval, Func<IEnumerable<string>, IEnumerable<T>> f)
        {
            var transformer = new TransformManyBlock<IEnumerable<string>, T>(f);
            var source = PollingSource.Create(pollingInterval, () => Process.GetProcesses().Select(p => p.ProcessName));
            source.LinkTo(transformer);
            transformer.Completion.ContinueWith(_ => source.Complete());
            return transformer;
        }

        public void Dispose()
        {
            RunningSimSource.Complete();
        }
    }
}
