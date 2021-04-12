using Xunit;
using RaceDirector.Pipeline;
using System.Threading.Tasks.Dataflow;
using System;

namespace Tests.Pipeline
{
    public class GameMonitorTest
    {
        private static TimeSpan Timeout = TimeSpan.FromSeconds(1);

        [Fact]
        public void OutputsNoGameWhenNoneRunning()
        {
            var gm = new GameMonitor();

            var buffer = new BufferBlock<RunningGame>();
            gm.RunningGameSource.LinkTo(buffer);

            Assert.Null(buffer.Receive(Timeout).name);
        }
    }
}
