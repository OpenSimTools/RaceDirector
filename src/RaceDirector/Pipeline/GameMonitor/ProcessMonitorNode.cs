using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace RaceDirector.Pipeline.GameMonitor
{
    public class ProcessMonitorNode : INode
    {
        private readonly ILogger<ProcessMonitorNode> _logger;

        public record Config(TimeSpan PollingInterval); // TODO remove when config done

        public IObservable<RunningGame> RunningGameSource
        {
            get => _lazyRunningGameSource.Value;
        }

        private readonly Lazy<IObservable<RunningGame>> _lazyRunningGameSource;

        public ProcessMonitorNode(ILogger<ProcessMonitorNode> logger, Config config, IEnumerable<IGameProcessInfo> gameProcessInfos)
        {
            _logger = logger;
            _lazyRunningGameSource =
                new Lazy<IObservable<RunningGame>>(() => GameProcessPoller(config, gameProcessInfos).Publish().RefCount());
        }

        private IObservable<RunningGame> GameProcessPoller(Config config, IEnumerable<IGameProcessInfo> gameProcessInfos)
        {
            // TODO Write this in a simpler way
            // TODO Add BehaviorSubject to emit latest element on subscribe or support ConnectableObservables in the framework?
            Dictionary<string, string> gameByProcess = GameByProcess(gameProcessInfos);
            Func<IEnumerable<string>, IEnumerable<string?>> keepOne = new KeepOne<string>(gameByProcess.Keys).Call;

            return ObservableInterval(config)
                .Select(_ => CurrentProcessNames())
                .SelectMany(processNames =>
                    keepOne(processNames).Select(processName =>
                    {
                        if (processName == null)
                        {
                            _logger.LogInformation("The game has been closed");
                            return new RunningGame(null);
                        }

                        var game = gameByProcess.GetValueOrDefault(processName);
                        _logger.LogInformation("Found game {string?} (process {string?})", game, processName);
                        return new RunningGame(game);
                    })
                );
        }

        // The Rx.NET framework is not easily testable
        protected virtual IObservable<long> ObservableInterval(Config config) =>
            Observable.Interval(config.PollingInterval);

        protected virtual IEnumerable<string> CurrentProcessNames() =>
            Process.GetProcesses().Select(p => p.ProcessName);

        private static Dictionary<string, string> GameByProcess(IEnumerable<IGameProcessInfo> gameProcessInfos)
        {
            return gameProcessInfos
                .SelectMany(gpi => gpi.GameProcessNames.Select(p => KeyValuePair.Create(p, gpi.GameName)))
                .ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
