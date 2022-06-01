using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using Xunit.Categories;
using RaceDirector.Pipeline.GameMonitor;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Reactive.Testing;
using Moq;
using RaceDirector.Pipeline;
using RaceDirector.Pipeline.Games;
using RaceDirector.Pipeline.Telemetry;
using RaceDirector.Pipeline.Telemetry.V0;
using RaceDirector.Plugin.HUD.Pipeline;
using RaceDirector.Plugin.HUD.Server;

namespace RaceDirector.Tests.Pipeline
{
    [IntegrationTest]
    public class DecoupledAcceptanceTest : ReactiveTest
    {
        [Fact]
        public void Lifecycle()
        {
            var testScheduler = new TestScheduler();

            var fakeGame = new FakeGame(callCount =>
                testScheduler.CreateColdObservable(FakeTelemetry(3, callCount, 0.1))
            );

            var fakeProcesses = new Dictionary<long, string[]>
            {
                [20] = fakeGame.GameProcessNames,
                [40] = Array.Empty<string>(),
                [60] = fakeGame.GameProcessNames,
                [80] = Array.Empty<string>()
            };

            var wsServerMock = new Mock<IWsServer<IGameTelemetry>>();

            PipelineBuilder.LinkNodes(new INode[]
                {
                    new FakeProcessMonitorNode(new[] {fakeGame}, fakeProcesses, testScheduler),
                    new TelemetryReaderNode(new[] {fakeGame}),
                    new WebSocketTelemetryNode(new[] {wsServerMock.Object})
                }
            );

            // TODO write proper assertions
            testScheduler.AdvanceTo(100);
            wsServerMock.Verify(s => s.Start());
        }

        Recorded<Notification<TestGameTelemetry>>[] FakeTelemetry(long tickIncrements, double baseValue,
            double valueIncrements)
        {
            var maxGenerated = 10;
            return Enumerable.Range(1, maxGenerated)
                .Select(i => OnNext(i * tickIncrements, new TestGameTelemetry(baseValue + i * valueIncrements)))
                .ToArray();
        }
    }

    record FakeGame(Func<double, IObservable<IGameTelemetry>> CreateTelemetrySourceF) : IGame
    {
        public string GameName
        {
            get => "TestGame";
        }

        public string[] GameProcessNames
        {
            get => new[] {"TestProcess.exe"};
        }

        private int _createTelemetryCallCounter = 0;

        public IObservable<IGameTelemetry> CreateTelemetrySource()
        {
            double callCount = Interlocked.Add(ref _createTelemetryCallCounter, 1);
            return CreateTelemetrySourceF(callCount);
        }
    }

    public record TestGameTelemetry(double Identifier) : IGameTelemetry
    {
        public GameState GameState
        {
            get => GameState.Menu;
        }

        public bool UsingVR
        {
            get => false;
        }

        public IEvent? Event
        {
            get => null;
        }

        public ISession? Session
        {
            get => null;
        }

        public IVehicle[] Vehicles
        {
            get => Array.Empty<IVehicle>();
        }

        public IFocusedVehicle? FocusedVehicle
        {
            get => null;
        }

        public IPlayer? Player
        {
            get => null;
        }
    }

    public class FakeProcessMonitorNode : ProcessMonitorNode
    {
        private readonly Dictionary<long, string[]> _simulatedProcessNames;
        private readonly TestScheduler _testScheduler;

        public FakeProcessMonitorNode(IEnumerable<IGameProcessInfo> gameProcessInfos,
            Dictionary<long, string[]> simulatedProcessNames, TestScheduler testScheduler) :
            base(NullLogger<ProcessMonitorNode>.Instance, new Config(TimeSpan.FromTicks(1)), gameProcessInfos)
        {
            _simulatedProcessNames = simulatedProcessNames;
            _testScheduler = testScheduler;
        }

        protected override IEnumerable<string> CurrentProcessNames()
        {
            var currentTicks = _testScheduler.Now.Ticks;
            var maxPastTickInSimulation = _simulatedProcessNames
                .Select(p => p.Key)
                .Where(t => t <= currentTicks)
                .DefaultIfEmpty(-1)
                .Max();
            return _simulatedProcessNames.GetValueOrDefault(maxPastTickInSimulation, Array.Empty<string>());
        }

        protected override IObservable<long> ObservableInterval(Config config) =>
            Observable.Interval(config.PollingInterval, _testScheduler);
    }
}