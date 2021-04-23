using RaceDirector.Pipeline.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks.Dataflow;

namespace RaceDirector.Pipeline.GameMonitor
{
    public class ProcessMonitorNode : IDisposable
    {
        public ISourceBlock<RunningGame> RunningGameSource
        {
            get;
        }

        public record Config(string[] GameNames, TimeSpan PollingInterval);

        public ProcessMonitorNode(Config config)
        {
            RunningGameSource = GameProcessPoller(config);
        }

        private ISourceBlock<RunningGame> GameProcessPoller(Config config)
        {
            Func<IEnumerable<string>, IEnumerable<string?>> keepOne = new KeepOne<string>(config.GameNames).Call;
            var transformer = new TransformManyBlock<IEnumerable<string>, RunningGame>(
                processNames => keepOne(processNames).Select(name => new RunningGame(name))
            );
            var source = PollingSource.Create(config.PollingInterval, () => Process.GetProcesses().Select(p => p.ProcessName));
            source.LinkTo(transformer);
            transformer.Completion.ContinueWith(_ => source.Complete());
            return transformer;
        }

        public void Dispose()
        {
            RunningGameSource.Complete();
        }
    }
}
