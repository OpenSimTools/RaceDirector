using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading;
using Microsoft.Reactive.Testing;
using Moq;
using RaceDirector.Pipeline;
using RaceDirector.Pipeline.GameMonitor;
using RaceDirector.Pipeline.Games;
using RaceDirector.Pipeline.Telemetry;
using RaceDirector.Pipeline.Telemetry.V0;
using RaceDirector.HUD.Pipeline;
using RaceDirector.Remote.Networking.Server;
using RaceDirector.Tests.Pipeline.Utils;
using Xunit;
using Xunit.Categories;

namespace RaceDirector.Tests.Pipeline;

[IntegrationTest]
public class DecoupledAcceptanceTest : ReactiveTest
{
    [Fact]
    public void Lifecycle()
    {
        var testScheduler = new TestScheduler();

        var fakeGame = new FakeGame(callCount =>
            testScheduler.CreateColdObservable(FakeTelemetry(3, 50, callCount, 0.01))
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

        testScheduler.AdvanceTo(20);
        wsServerMock.Verify(s => s.Start());
        wsServerMock.VerifyNoOtherCalls();
        wsServerMock.Reset();

        testScheduler.AdvanceTo(39);
        wsServerMock.Verify(s => s.Multicast(Match.Create<IGameTelemetry>(_ => true)), Times.Exactly(20 / 3));
        wsServerMock.Reset();

        testScheduler.AdvanceTo(40);
        wsServerMock.Verify(s => s.Stop());
        testScheduler.AdvanceTo(59);
        wsServerMock.VerifyNoOtherCalls();
        wsServerMock.Reset();

        testScheduler.AdvanceTo(60);
        wsServerMock.Verify(s => s.Start());
        wsServerMock.VerifyNoOtherCalls();
        wsServerMock.Reset();

        testScheduler.AdvanceTo(79);
        wsServerMock.Verify(s => s.Multicast(Match.Create<IGameTelemetry>(_ => true)), Times.Exactly(20 / 3));
        wsServerMock.Reset();

        testScheduler.AdvanceTo(80);
        wsServerMock.Verify(s => s.Stop());
        testScheduler.AdvanceTo(99);
        wsServerMock.VerifyNoOtherCalls();
        wsServerMock.Reset();
    }

    private Recorded<Notification<TestGameTelemetry>>[] FakeTelemetry(long tickIncrements, long maxTick,
        double baseValue, double valueIncrements)
    {
        var maxGenerated = 10;
        return Enumerable.Range(1, maxGenerated)
            .TakeWhile(i => i * tickIncrements <= maxTick)
            .Select(i => OnNext(i * tickIncrements, new TestGameTelemetry(baseValue + i * valueIncrements)))
            .ToArray();
    }
}

internal record FakeGame(Func<double, IObservable<IGameTelemetry>> CreateTelemetryObservableF) : IGame
{
    private int _createTelemetryCallCounter;

    public string GameName => "TestGame";

    public string[] GameProcessNames => new[] {"TestProcess.exe"};

    public IObservable<IGameTelemetry> CreateTelemetryObservable()
    {
        double callCount = Interlocked.Add(ref _createTelemetryCallCounter, 1);
        return CreateTelemetryObservableF(callCount);
    }
}

public record TestGameTelemetry(double Identifier) : IGameTelemetry
{
    public GameState GameState => GameState.Menu;

    public bool? UsingVR => null;

    public IEvent? Event => null;

    public ISession? Session => null;

    public IVehicle[] Vehicles => Array.Empty<IVehicle>();

    public IFocusedVehicle? FocusedVehicle => null;

    public IPlayer? Player => null;
}

public class FakeProcessMonitorNode : TestProcessMonitorNode
{
    private readonly Dictionary<long, string[]> _simulatedProcessNames;

    public FakeProcessMonitorNode(IEnumerable<IGameProcessInfo> gameProcessInfos,
        Dictionary<long, string[]> simulatedProcessNames, TestScheduler testScheduler) :
        base(new Config { PollingInterval = TimeSpan.FromTicks(1) }, gameProcessInfos, testScheduler)
    {
        _simulatedProcessNames = simulatedProcessNames;
    }

    protected override IEnumerable<string> CurrentProcessNames()
    {
        var currentTicks = TestScheduler.Now.Ticks;
        var maxPastTickInSimulation = _simulatedProcessNames
            .Select(p => p.Key)
            .Where(t => t <= currentTicks)
            .DefaultIfEmpty(-1)
            .Max();
        return _simulatedProcessNames.GetValueOrDefault(maxPastTickInSimulation, Array.Empty<string>());
    }
}