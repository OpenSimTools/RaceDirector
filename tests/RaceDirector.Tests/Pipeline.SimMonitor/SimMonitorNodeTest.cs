using Xunit;
using System.Threading.Tasks.Dataflow;
using System;
using System.Diagnostics;
using Xunit.Categories;
using RaceDirector.Pipeline.SimMonitor;

namespace Tests.Pipeline.SimMonitor
{
     [IntegrationTest]
     public class SimMonitorNodeTest
    {
        private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan Timeout = PollingInterval * 3;
        private static readonly int DecimalDigitPrecision = 1;

        private static readonly string ProcessName = "RaceDirector.Tests.Ext.Process";
        private static readonly string ProcessArgs = Timeout.Multiply(3).Seconds.ToString();

        [Fact]
        public void PollsProcessesAtTheConfiguredInterval()
        {
            var source = SimMonitorNode.ProcessPoller(PollingInterval, _ => new[] { DateTime.Now });
            var start = DateTime.Now;
            for (var i = 0; i < (Timeout / PollingInterval); i++)
            {
                var timePassed = source.Receive(Timeout).Subtract(start);
                Assert.Equal(i * PollingInterval.TotalSeconds, timePassed.TotalSeconds, DecimalDigitPrecision);
            }
            source.Complete();
        }

        [Fact]
        public void OutputsSimNameWhenRunning()
        {
            var config = new SimMonitorNode.Config(new[] { ProcessName }, PollingInterval);
            using (var simMonitorNode = new SimMonitorNode(config))
            {
                var source = simMonitorNode.RunningSimSource;
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
