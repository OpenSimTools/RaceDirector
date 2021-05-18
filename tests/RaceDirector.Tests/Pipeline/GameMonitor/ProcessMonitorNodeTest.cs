using Xunit;
using System.Threading.Tasks.Dataflow;
using System;
using System.Diagnostics;
using Xunit.Categories;
using RaceDirector.Pipeline.GameMonitor;

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


        [Fact]
        public void OutputGameNameWhenProcessRunning()
        {
            var gameProcessInfos = new [] {
                new GameProcessInfo(GameName, new string[] { ProcessName })
            };
            var config = new ProcessMonitorNode.Config(PollingInterval);
            using (var processMonitorNode = new ProcessMonitorNode(config, gameProcessInfos))
            {
                var source = processMonitorNode.RunningGameSource;
                using (new RunningProcess(ProcessName, ProcessArgs))
                {
                    Assert.Equal(GameName, source.Receive(Timeout).Name);
                }
                Assert.Null(source.Receive(Timeout).Name);
            }
        }

        private record GameProcessInfo(string GameName, string[] GameProcessNames) : IGameProcessInfo;

        private record RunningProcess(string Name, string Args) : IDisposable
        {
            public static string DotExe = ".exe";

            private readonly Process process = Process.Start(Name + DotExe, Args);

            public void Dispose()
            {
                process.Kill();
            }
        }
    }
}
