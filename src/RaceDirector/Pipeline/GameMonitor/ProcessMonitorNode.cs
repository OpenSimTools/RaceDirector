using Microsoft.Extensions.Logging;
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
        private readonly ILogger<ProcessMonitorNode> _logger;

        public record Config(TimeSpan PollingInterval); // TODO remove when config done

        public ISourceBlock<RunningGame> RunningGameSource
        {
            get;
        }

        public ProcessMonitorNode(ILogger<ProcessMonitorNode> logger, Config config, IEnumerable<IGameProcessInfo> gameProcessInfos)
        {
            _logger = logger;
            RunningGameSource = GameProcessPoller(config, gameProcessInfos);
        }

        private ISourceBlock<RunningGame> GameProcessPoller(Config config, IEnumerable<IGameProcessInfo> gameProcessInfos)
        {
            Dictionary<string, string> gameByProcess = GameByProcess(gameProcessInfos);
            Func<IEnumerable<string>, IEnumerable<string?>> keepOne = new KeepOne<string>(gameByProcess.Keys).Call;
            var transformer = new TransformManyBlock<IEnumerable<string>, RunningGame>(
                processNames => keepOne(processNames).Select(processName =>
                {
                    if (processName == null)
                    {
                        _logger.LogInformation("No matching game");
                        return new RunningGame(null);
                    }

                    var game = gameByProcess.GetValueOrDefault(processName);
                    _logger.LogInformation("Found game {string?} for process {string?}", game, processName);
                    return new RunningGame(game);
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
