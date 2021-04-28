using RaceDirector.Pipeline.GameMonitor;
using RaceDirector.Pipeline.Telemetry;
using RaceDirector.Pipeline.Telemetry.V0;
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

        [Fact]
        public void DoesNotEmitWhenGameNotMatching()
        {
            var trn = new TelemetryReaderNode(new ITelemetrySourceFactory[0]);
            trn.RunningGameTarget.Post(new RunningGame(null));
            trn.RunningGameTarget.Post(new RunningGame("any"));
            Assert.Throws<TimeoutException>(() => trn.LiveTelemetrySource.Receive(Timeout));
        }

        [Fact]
        public void SwitchesSourcesWhenGameChanges()
        {
            var trn = new TelemetryReaderNode(new[]
            {
                new TestTelemetrySourceFactory("a", Telemetry1, Telemetry2),
                new TestTelemetrySourceFactory("b", Telemetry3)
            });
            trn.RunningGameTarget.Post(new RunningGame("a"));
            Assert.Equal(Telemetry1, trn.LiveTelemetrySource.Receive(Timeout));
            Assert.Equal(Telemetry2, trn.LiveTelemetrySource.Receive(Timeout));
            trn.RunningGameTarget.Post(new RunningGame("b"));
            Assert.Equal(Telemetry3, trn.LiveTelemetrySource.Receive(Timeout));
        }

        private record TestTelemetrySourceFactory(string gameName, params LiveTelemetry[] elements) : ITelemetrySourceFactory
        {
            public string GameName => gameName;

            public ISourceBlock<ILiveTelemetry> CreateTelemetrySource()
            {
                return StaticSourceBlock(elements);
            }
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
