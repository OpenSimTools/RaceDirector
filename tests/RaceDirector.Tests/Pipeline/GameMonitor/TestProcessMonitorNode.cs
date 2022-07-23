using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Reactive.Testing;
using RaceDirector.Pipeline.GameMonitor;

namespace RaceDirector.Tests.Pipeline.Utils;

/// <summary>
/// ProcessMonitorNode that runs on a test scheduler and null logger.
/// </summary>
public class TestProcessMonitorNode : ProcessMonitorNode
{
    protected readonly TestScheduler TestScheduler;

    public TestProcessMonitorNode(Config config, IEnumerable<IGameProcessInfo> gameProcessInfos, TestScheduler testScheduler) :
        base(NullLogger<ProcessMonitorNode>.Instance, config, gameProcessInfos)
    {
        TestScheduler = testScheduler;
    }

    protected override IObservable<long> ObservableInterval(Config config) =>
        Observable.Interval(config.PollingInterval, TestScheduler);
}