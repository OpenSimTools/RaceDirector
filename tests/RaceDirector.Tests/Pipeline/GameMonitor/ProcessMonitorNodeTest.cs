using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Reactive.Testing;
using RaceDirector.Pipeline.GameMonitor;
using RaceDirector.Tests.Pipeline.Utils;
using TestUtils;
using Xunit;
using Xunit.Categories;

namespace RaceDirector.Tests.Pipeline.GameMonitor;

[IntegrationTest, LocalTest]
public class ProcessMonitorNodeTest
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan Timeout = PollingInterval * 3;

    private static readonly string GameName = "TestGame";
    private static readonly string ProcessName = "RaceDirector.Tests.Ext.Process";
    private static readonly string ProcessArgs = Timeout.Multiply(3).Seconds.ToString();

    [Fact]
    public void OutputGameNameWhenProcessRunning()
    {
        var testScheduler = new TestScheduler();
        var gameProcessInfos = new[]
        {
            new GameProcessInfo(GameName, new[] {ProcessName})
        };
        var config = new ProcessMonitorNode.Config { PollingInterval = PollingInterval };
        var processMonitorNode = new TestProcessMonitorNode(config, gameProcessInfos, testScheduler);
        var observer = testScheduler.CreateObserver<RunningGame>();
        processMonitorNode.RunningGameObservable.Subscribe(observer);
        using (new RunningProcess(ProcessName, ProcessArgs))
        {
            testScheduler.AdvanceTo(PollingInterval.Ticks);
            var first = Assert.Single(observer.ReceivedValues());
            Assert.Equal(GameName, first.Name);
        }

        testScheduler.AdvanceBy(PollingInterval.Ticks);
        var second = Assert.Single(observer.ReceivedValues().Skip(1));
        Assert.Null(second.Name);
    }

    private record GameProcessInfo(string GameName, string[] GameProcessNames) : IGameProcessInfo;

    private record RunningProcess(string Name, string Args) : IDisposable
    {
        private const string DotExe = ".exe";

        private readonly Process _process = Process.Start(Name + DotExe, Args);

        public void Dispose()
        {
            _process.Kill();
        }
    }
}