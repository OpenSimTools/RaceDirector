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

        private static readonly string ProcessName = "RaceDirector.Tests.Ext.Process";
        private static readonly string ProcessArgs = Timeout.Multiply(3).Seconds.ToString();


        [Fact]
        public void OutputGameNameWhenProcessRunning()
        {
            var config = new ProcessMonitorNode.Config(new[] { ProcessName }, PollingInterval);
            using (var processMonitorNode = new ProcessMonitorNode(config))
            {
                var source = processMonitorNode.RunningGameSource;
                using (var process = new RunningProcess(ProcessName, ProcessArgs))
                {
                    Assert.Equal(process.Name, source.Receive(Timeout).Name);
                }
                Assert.Null(source.Receive(Timeout).Name);
            }
        }

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
