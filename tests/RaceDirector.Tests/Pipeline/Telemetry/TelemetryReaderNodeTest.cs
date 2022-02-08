using AutoBogus;
using AutoBogus.Moq;
using Microsoft.Reactive.Testing;
using RaceDirector.Pipeline.GameMonitor;
using RaceDirector.Pipeline.Telemetry;
using RaceDirector.Pipeline.Telemetry.V0;
using RaceDirector.Tests.Pipeline.Utils;
using System;
using System.Reactive.Linq;
using Xunit;
using Xunit.Categories;

namespace RaceDirector.Tests.Pipeline.Telemetry
{
    [IntegrationTest]
    public class TelemetryReaderNodeTest
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromMilliseconds(50);

        private static readonly IGameTelemetry[] Telemetry = AutoFaker.Generate<IGameTelemetry>(3, b => b.WithBinder<MoqBinder>()).ToArray();

        private TestScheduler _testScheduler;
        private ITestableObserver<IGameTelemetry> _testObserver;

        public TelemetryReaderNodeTest()
        {
            _testScheduler = new TestScheduler();
            _testObserver = _testScheduler.CreateObserver<IGameTelemetry>();
        }

        [Fact]
        public void DoesNotEmitWhenGameNotMatching()
        {
            var trn = new TelemetryReaderNode(Array.Empty<ITelemetrySourceFactory>());
            trn.GameTelemetrySource.Subscribe(_testObserver);

            trn.RunningGameTarget.OnNext(new RunningGame(null));
            trn.RunningGameTarget.OnNext(new RunningGame("any"));

            Assert.Empty(_testObserver.ReceivedValues());
        }

        [Fact]
        public void SwitchesSourcesWhenGameChanges()
        {
            var trn = new TelemetryReaderNode(new ITelemetrySourceFactory[]
            {
                    new TestTelemetrySourceFactory("a", Telemetry[0]),
                    new TestTelemetrySourceFactory("b", Telemetry[1], Telemetry[2])
            });
            trn.GameTelemetrySource.Subscribe(_testObserver);

            trn.RunningGameTarget.OnNext(new RunningGame("a"));
            Assert.Equal(new[] { Telemetry[0] }, _testObserver.ReceivedValues());

            trn.RunningGameTarget.OnNext(new RunningGame("b"));
            Assert.Equal(new[] { Telemetry[0], Telemetry[1], Telemetry[2] }, _testObserver.ReceivedValues());

            trn.RunningGameTarget.OnNext(new RunningGame("a"));
            Assert.Equal(new[] { Telemetry[0], Telemetry[1], Telemetry[2], Telemetry[0] }, _testObserver.ReceivedValues());
        }

        private record TestTelemetrySourceFactory(string GameName, params IGameTelemetry[] Elements) : ITelemetrySourceFactory
        {
            public IObservable<IGameTelemetry> CreateTelemetrySource()
            {
                return Elements.ToObservable();
            }
        }
    }
}
