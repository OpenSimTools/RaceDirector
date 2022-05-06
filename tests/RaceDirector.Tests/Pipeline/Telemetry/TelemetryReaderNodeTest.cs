using AutoBogus;
using AutoBogus.Moq;
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

        private static readonly IGameTelemetry[] Telemetry = AutoFaker.Generate<IGameTelemetry>(3, b => b.WithBinder<MoqBinder>()).ToArray();

        [Fact]
        public void DoesNotEmitWhenGameNotMatching()
        {
            var trn = new TelemetryReaderNode(new ITelemetrySourceFactory[0]);
            trn.RunningGameTarget.Post(new RunningGame(null));
            trn.RunningGameTarget.Post(new RunningGame("any"));
            Assert.Throws<TimeoutException>(() => trn.GameTelemetrySource.Receive(Timeout));
        }

        [Fact]
        public void SwitchesSourcesWhenGameChanges()
        {
            var trn = new TelemetryReaderNode(new[]
            {
                new TestTelemetrySourceFactory("a", Telemetry[0], Telemetry[1]),
                new TestTelemetrySourceFactory("b", Telemetry[2])
            });
            trn.RunningGameTarget.Post(new RunningGame("a"));
            Assert.Equal(Telemetry[0], trn.GameTelemetrySource.Receive(Timeout));
            Assert.Equal(Telemetry[1], trn.GameTelemetrySource.Receive(Timeout));
            trn.RunningGameTarget.Post(new RunningGame("b"));
            Assert.Equal(Telemetry[2], trn.GameTelemetrySource.Receive(Timeout));
        }

        private record TestTelemetrySourceFactory(string GameName, params IGameTelemetry[] Elements) : ITelemetrySourceFactory
        {
            public ISourceBlock<IGameTelemetry> CreateTelemetrySource()
            {
                return StaticSourceBlock(Elements);
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
