using RaceDirector.Pipeline.Utils;
using System;
using System.Threading.Tasks.Dataflow;
using Xunit;
using Xunit.Categories;

namespace RaceDirector.Tests.Pipeline.Utils
{
    [IntegrationTest]
    public class PollingSourceTest
    {
        private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan Timeout = PollingInterval * 3;

        private static readonly int DecimalDigitPrecision = 1;

        [Fact]
        public void PollsAtTheConfiguredInterval()
        {
            var source = PollingSource.Create(PollingInterval, () => DateTime.Now);
            var start = DateTime.Now;
            for (var i = 0; i < (Timeout / PollingInterval); i++)
            {
                var timePassed = source.Receive(Timeout).Subtract(start);
                Assert.Equal(i * PollingInterval.TotalSeconds, timePassed.TotalSeconds, DecimalDigitPrecision);
            }
            source.Complete();
        }
    }
}
