using RaceDirector.Pipeline.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks.Dataflow;

namespace RaceDirector.Pipeline.GameMonitor
{
    public class ProcessMonitorNode : INode, IDisposable
    {
        public record Config(TimeSpan PollingInterval); // TODO remove when config done

        public ISourceBlock<RunningGame> RunningGameSource
        {
            get;
        }

        public ProcessMonitorNode(Config config, IEnumerable<IGameProcessInfo> gameProcessInfos)
        {
            RunningGameSource = GameProcessPoller(config, gameProcessInfos);
        }

        private ISourceBlock<RunningGame> GameProcessPoller(Config config, IEnumerable<IGameProcessInfo> gameProcessInfos)
        {
            Dictionary<string, string> gameByProcess = GameByProcess(gameProcessInfos);
            Func<IEnumerable<string>, IEnumerable<string?>> keepOne = new KeepOne<string>(gameByProcess.Keys).Call;
            var transformer = new TransformManyBlock<IEnumerable<string>, RunningGame>(
                processNames => keepOne(processNames).Select(processName => {
                    if (processName == null)
                        return new RunningGame(null);
                    else
                        return new RunningGame(gameByProcess.GetValueOrDefault(processName));
                })
            );
            var source = PollingSource.Create(config.PollingInterval, () => Process.GetProcesses().Select(p => p.ProcessName));
            source.LinkTo(transformer);
            transformer.Completion.ContinueWith(_ => source.Complete());
            return transformer;
        }

        private static Dictionary<string, string> GameByProcess(IEnumerable<IGameProcessInfo> gameProcessInfos)
        {
            return gameProcessInfos
                .SelectMany(gpi => gpi.GameProcessNames.Select(p => KeyValuePair.Create(p, gpi.GameName)))
                .ToDictionary(x => x.Key, x => x.Value);
        }

        public void Dispose()
        {
            RunningGameSource.Complete();
        }
    }
}
