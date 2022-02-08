using Xunit;
using System;
using System.Diagnostics;
using Xunit.Categories;
using RaceDirector.Pipeline.GameMonitor;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Reactive.Linq;

namespace RaceDirector.Tests.Pipeline.GameMonitor
{
     [IntegrationTest]
     public class ProcessMonitorNodeTest
    {
        private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan Timeout = PollingInterval * 3;

        private static readonly string GameName = "TestGame";
        private static readonly string ProcessName = "RaceDirector.Tests.Ext.Process";
        private static readonly string ProcessArgs = Timeout.Multiply(3).Seconds.ToString();

        private static readonly ILogger<ProcessMonitorNode> TestLogger = NullLogger<ProcessMonitorNode>.Instance;

        [Fact]
        public void OutputGameNameWhenProcessRunning()
        {
            var gameProcessInfos = new [] {
                new GameProcessInfo(GameName, new[] { ProcessName })
            };
            var config = new ProcessMonitorNode.Config(PollingInterval);
            var processMonitorNode = new ProcessMonitorNode(TestLogger, config, gameProcessInfos);
            var source = processMonitorNode.RunningGameSource.Timeout(Timeout);
            using (new RunningProcess(ProcessName, ProcessArgs))
            {
                var first = source.First(); // FIXME
                Assert.Equal(GameName, first.Name);
            }
            var second = source.First(); // FIXME
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
}
