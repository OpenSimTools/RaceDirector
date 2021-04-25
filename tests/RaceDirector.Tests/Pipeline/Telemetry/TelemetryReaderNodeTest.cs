using RaceDirector.Pipeline.GameMonitor;
using RaceDirector.Pipeline.Telemetry;
using System;
using System.Threading.Tasks.Dataflow;
using Xunit;
using Xunit.Categories;

namespace RaceDirector.Tests.Pipeline.Telemetry
{
    [IntegrationTest]
    public class TelemetryReaderNodeTest
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromMilliseconds(50);

        private static readonly LiveTelemetry Telemetry1 = new LiveTelemetry(TimeSpan.FromMilliseconds(1));
        private static readonly LiveTelemetry Telemetry2 = new LiveTelemetry(TimeSpan.FromMilliseconds(2));
        private static readonly LiveTelemetry Telemetry3 = new LiveTelemetry(TimeSpan.FromMilliseconds(3));
        private static readonly LiveTelemetry Telemetry4 = new LiveTelemetry(TimeSpan.FromMilliseconds(4));

        [Fact]
        public void DoesNotEmitWhenGameNotRunning()
        {
            var trn = new TelemetryReaderNode(_ => StaticSourceBlock(Telemetry1));
            Assert.Throws<TimeoutException>(() => trn.LiveTelemetrySource.Receive(Timeout));
        }

        [Fact]
        public void DoesNotEmitWhenGameNotMatching()
        {
            var trn = new TelemetryReaderNode(_ => null);
            trn.RunningGameTarget.Post(new RunningGame("any"));
            Assert.Throws<TimeoutException>(() => trn.LiveTelemetrySource.Receive(Timeout));
        }

        [Fact]
        public void SwitchesSourcesWhenGameChanges()
        {
            var trn = new TelemetryReaderNode(rg =>
            {
                switch (rg)
                {
                    case "a":
                        return StaticSourceBlock(Telemetry1, Telemetry2);
                    case "b":
                        return StaticSourceBlock(Telemetry3);
                    default:
                        return null;
                }
            });
            trn.RunningGameTarget.Post(new RunningGame("a"));
            Assert.Equal(Telemetry1, trn.LiveTelemetrySource.Receive(Timeout));
            Assert.Equal(Telemetry2, trn.LiveTelemetrySource.Receive(Timeout));
            trn.RunningGameTarget.Post(new RunningGame("b"));
            Assert.Equal(Telemetry3, trn.LiveTelemetrySource.Receive(Timeout));
        }

        private static ISourceBlock<T> StaticSourceBlock<T>(params T[] elements)
        {
            var bufferBlock = new BufferBlock<T>();
            foreach (var e in elements)
                bufferBlock.Post(e);
            return bufferBlock;
        }
    }
}
