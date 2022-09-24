using System.Reactive.Subjects;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Reactive.Testing;
using Moq;
using RaceDirector.DeviceIO.Pipeline;
using RaceDirector.Pipeline.Telemetry;
using RaceDirector.Pipeline.Telemetry.Physics;
using RaceDirector.Pipeline.Telemetry.V0;
using RaceDirector.PitCrew.Pipeline.Games;
using RaceDirector.PitCrew.Protocol;
using Xunit;
using Xunit.Categories;
using PitMenu = RaceDirector.Pipeline.Telemetry.PitMenu;

namespace PitCrew.Plugin.Tests.Pipeline.Games;

[UnitTest]
public class ACCPitMenuNavigatorTest : ReactiveTest
{
    #region GoToFuel

    [Fact]
    public void GoToFuelPitStrategyMenuIsPresent()
    {
        const int telemetryOffset = 5;
        
        var gameTelemetryObservable = _testScheduler.CreateHotObservable(
            OnNext(0, TelemetryWithPitFuelToAdd(ICapacity.FromL(1))),
            OnNext(Subscribed + telemetryOffset, TelemetryWithPitFuelToAdd(ICapacity.FromL(0))),
            OnCompleted<IGameTelemetry>(Disposed)
        );
        var replySubject = new ReplaySubject<IGameTelemetry>(1);
        gameTelemetryObservable.Subscribe(replySubject);

        _testScheduler.Start(() =>
            _pmn.GoToFuel(replySubject)
        ).Messages.AssertEqual(
            OnNext(Subscribed + 0, GameAction.PitMenuDown),
            OnNext(Subscribed + 0, GameAction.PitMenuDown),
            OnNext(Subscribed + 0, GameAction.PitMenuLeft),
            OnNext(Subscribed + 0, GameAction.PitMenuLeft),
            OnNext(Subscribed + 0, GameAction.PitMenuRight),
            OnCompleted<GameAction>(Subscribed + telemetryOffset)
        );
    }
    
    [Fact]
    public void GoToFuelPitStrategyMenuIsNotPresent()
    {
        const int telemetryOffset = 5;
        
        var gameTelemetryObservable = _testScheduler.CreateHotObservable(
            OnNext(0, TelemetryWithPitFuelToAdd(ICapacity.FromL(1))),
            OnNext(Subscribed + 1 * TimeoutTicks + telemetryOffset, TelemetryWithPitFuelToAdd(ICapacity.FromL(0))),
            OnCompleted<IGameTelemetry>(Disposed)
        );
        var replySubject = new ReplaySubject<IGameTelemetry>(1);
        gameTelemetryObservable.Subscribe(replySubject);

        _testScheduler.Start(() =>
            _pmn.GoToFuel(replySubject)
        ).Messages.AssertEqual(
            OnNext(Subscribed + 0, GameAction.PitMenuDown),
            OnNext(Subscribed + 0, GameAction.PitMenuDown),
            OnNext(Subscribed + 0, GameAction.PitMenuLeft),
            OnNext(Subscribed + 0, GameAction.PitMenuLeft),
            OnNext(Subscribed + 0, GameAction.PitMenuRight),
            OnNext(Subscribed + TimeoutTicks, GameAction.PitMenuRight),
            OnNext(Subscribed + TimeoutTicks, GameAction.PitMenuUp),
            OnNext(Subscribed + TimeoutTicks, GameAction.PitMenuLeft),
            OnNext(Subscribed + TimeoutTicks, GameAction.PitMenuLeft),
            OnNext(Subscribed + TimeoutTicks, GameAction.PitMenuRight),
            OnCompleted<GameAction>(Subscribed + TimeoutTicks + telemetryOffset)
        );
    }
    
    [Fact]
    public void GoToFuelUnknownState()
    {
        var gameTelemetryObservable = _testScheduler.CreateHotObservable(
            OnNext(0, TelemetryWithPitFuelToAdd(ICapacity.FromL(1))),
            OnCompleted<IGameTelemetry>(Disposed)
        );
        var replySubject = new ReplaySubject<IGameTelemetry>(1);
        gameTelemetryObservable.Subscribe(replySubject);

        _testScheduler.Start(() =>
            _pmn.GoToFuel(replySubject)
        ).Messages.AssertEqual(
            OnNext(Subscribed + 0, GameAction.PitMenuDown),
            OnNext(Subscribed + 0, GameAction.PitMenuDown),
            OnNext(Subscribed + 0, GameAction.PitMenuLeft),
            OnNext(Subscribed + 0, GameAction.PitMenuLeft),
            OnNext(Subscribed + 0, GameAction.PitMenuRight),
            OnNext(Subscribed + TimeoutTicks, GameAction.PitMenuRight),
            OnNext(Subscribed + TimeoutTicks, GameAction.PitMenuUp),
            OnNext(Subscribed + TimeoutTicks, GameAction.PitMenuLeft),
            OnNext(Subscribed + TimeoutTicks, GameAction.PitMenuLeft),
            OnNext(Subscribed + TimeoutTicks, GameAction.PitMenuRight),
            OnError<GameAction>(Subscribed + 2 * TimeoutTicks,
                _ => _.Message.Contains("unknown state"))
        );
    }

    #endregion
    
    #region SetFuel

    [Fact]
    public void DontChangeFuelIfNotPresentInTelemetry()
    {
        var gameTelemetryObservable = _testScheduler.CreateColdObservable(
            OnNext<IGameTelemetry>(TelemetryTicks, GameTelemetry.Empty),
            OnCompleted<IGameTelemetry>(TelemetryTicks + 1)
        );

        var output = _testScheduler
            .Start(() => _pmn.SetFuel(7, gameTelemetryObservable, NullLogger.Instance));

        output.Messages.AssertEqual(
            OnCompleted<GameAction>(Subscribed + TelemetryTicks)
        );
    }

    [Fact]
    public void AddFuelIfCurrentIsLower()
    {
        var telemetryFuel = 5;

        var gameTelemetryObservable = _testScheduler.CreateColdObservable(
            OnNext(TelemetryTicks, TelemetryWithPitFuelToAdd(ICapacity.FromL(telemetryFuel))),
            OnCompleted<IGameTelemetry>(TelemetryTicks + 1)
        );

        var output = _testScheduler
            .Start(() => _pmn.SetFuel(telemetryFuel + 2, gameTelemetryObservable, NullLogger.Instance));

        output.Messages.AssertEqual(
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuRight),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuRight),
            OnCompleted<GameAction>(Subscribed + TelemetryTicks)
        );
    }
    
    [Fact]
    public void RemovesFuelIfCurrentIsHigher()
    {
        var telemetryFuel = 5;

        var gameTelemetryObservable = _testScheduler.CreateColdObservable(
            OnNext(TelemetryTicks, TelemetryWithPitFuelToAdd(ICapacity.FromL(telemetryFuel))),
            OnCompleted<IGameTelemetry>(TelemetryTicks + 1)
        );

        var output = _testScheduler
            .Start(() => _pmn.SetFuel(telemetryFuel - 2, gameTelemetryObservable, NullLogger.Instance));

        output.Messages.AssertEqual(
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuLeft),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuLeft),
            OnCompleted<GameAction>(Subscribed + TelemetryTicks)
        );
    }
    
    #endregion
    
    #region GoToKnownTireState
    
    [Fact]
    public void GoToKnownTireStateTireChangeOpenAndDrySet()
    {
        const int telemetryOffset = 5;
        
        var gameTelemetryObservable = _testScheduler.CreateHotObservable(
            OnNext(0, TelemetryWithTireSet(1)),
            OnNext(Subscribed + telemetryOffset, TelemetryWithTireSet(2)),
            OnCompleted<IGameTelemetry>(Disposed)
        );
        var replySubject = new ReplaySubject<IGameTelemetry>(1);
        gameTelemetryObservable.Subscribe(replySubject);

        _testScheduler.Start(() =>
            _pmn.GoToKnownTireState(replySubject)
        ).Messages.AssertEqual(
            OnNext(Subscribed + 0, GameAction.PitMenuDown),
            OnNext(Subscribed + 0, GameAction.PitMenuDown),
            OnNext(Subscribed + 0, GameAction.PitMenuRight),
            OnCompleted<GameAction>(Subscribed + telemetryOffset)
        );
    }
    
    [Fact]
    public void GoToKnownTireStateTireChangeOpenAndWet()
    {
        const int telemetryOffset = 5;
        
        var gameTelemetryObservable = _testScheduler.CreateHotObservable(
            OnNext(0, TelemetryWithTireSet(1)),
            OnNext(Subscribed + 1 * TimeoutTicks + telemetryOffset, TelemetryWithTireSet(2)),
            OnCompleted<IGameTelemetry>(Disposed)
        );
        var replySubject = new ReplaySubject<IGameTelemetry>(1);
        gameTelemetryObservable.Subscribe(replySubject);

        _testScheduler.Start(() =>
            _pmn.GoToKnownTireState(replySubject)
        ).Messages.AssertEqual(
            OnNext(Subscribed + 0, GameAction.PitMenuDown),
            OnNext(Subscribed + 0, GameAction.PitMenuDown),
            OnNext(Subscribed + 0, GameAction.PitMenuRight),
            OnNext(Subscribed + 1 * TimeoutTicks, GameAction.PitMenuLeft),
            OnNext(Subscribed + 1 * TimeoutTicks, GameAction.PitMenuUp),
            OnNext(Subscribed + 1 * TimeoutTicks, GameAction.PitMenuRight),
            OnCompleted<GameAction>(Subscribed + 1 * TimeoutTicks + telemetryOffset)
        );
    }
    
    [Fact]
    public void GoToKnownTireStateTireChangeClosedAndDry()
    {
        const int telemetryOffset = 5;
        
        var gameTelemetryObservable = _testScheduler.CreateHotObservable(
            OnNext(0, TelemetryWithTireSet(1)),
            OnNext(Subscribed + 2 * TimeoutTicks + telemetryOffset, TelemetryWithTireSet(2)),
            OnCompleted<IGameTelemetry>(Disposed)
        );
        var replySubject = new ReplaySubject<IGameTelemetry>(1);
        gameTelemetryObservable.Subscribe(replySubject);

        _testScheduler.Start(() =>
            _pmn.GoToKnownTireState(replySubject)
        ).Messages.AssertEqual(
            OnNext(Subscribed + 0, GameAction.PitMenuDown),
            OnNext(Subscribed + 0, GameAction.PitMenuDown),
            OnNext(Subscribed + 0, GameAction.PitMenuRight),
            OnNext(Subscribed + 1 * TimeoutTicks, GameAction.PitMenuLeft),
            OnNext(Subscribed + 1 * TimeoutTicks, GameAction.PitMenuUp),
            OnNext(Subscribed + 1 * TimeoutTicks, GameAction.PitMenuRight),
            OnNext(Subscribed + 2 * TimeoutTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + 2 * TimeoutTicks, GameAction.PitMenuRight),
            OnCompleted<GameAction>(Subscribed + 2 * TimeoutTicks + telemetryOffset)
        );
    }
    
    [Fact]
    public void GoToKnownTireStateTireChangeClosedAndWet()
    {
        const int telemetryOffset = 5;
        
        var gameTelemetryObservable = _testScheduler.CreateHotObservable(
            OnNext(0, TelemetryWithTireSet(1)),
            OnNext(Subscribed + 3 * TimeoutTicks + telemetryOffset, TelemetryWithTireSet(2)),
            OnCompleted<IGameTelemetry>(Disposed)
        );
        var replySubject = new ReplaySubject<IGameTelemetry>(1);
        gameTelemetryObservable.Subscribe(replySubject);

        _testScheduler.Start(() =>
            _pmn.GoToKnownTireState(replySubject)
        ).Messages.AssertEqual(
            OnNext(Subscribed + 0, GameAction.PitMenuDown),
            OnNext(Subscribed + 0, GameAction.PitMenuDown),
            OnNext(Subscribed + 0, GameAction.PitMenuRight),
            OnNext(Subscribed + 1 * TimeoutTicks, GameAction.PitMenuLeft),
            OnNext(Subscribed + 1 * TimeoutTicks, GameAction.PitMenuUp),
            OnNext(Subscribed + 1 * TimeoutTicks, GameAction.PitMenuRight),
            OnNext(Subscribed + 2 * TimeoutTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + 2 * TimeoutTicks, GameAction.PitMenuRight),
            OnNext(Subscribed + 3 * TimeoutTicks, GameAction.PitMenuLeft),
            OnNext(Subscribed + 3 * TimeoutTicks, GameAction.PitMenuUp),
            OnNext(Subscribed + 3 * TimeoutTicks, GameAction.PitMenuRight),
            OnCompleted<GameAction>(Subscribed + 3 * TimeoutTicks + telemetryOffset)
        );
    }

    [Fact]
    public void GoToKnownTireStateSomethingWentWrong()
    {
        var gameTelemetryObservable = _testScheduler.CreateHotObservable(
            OnCompleted<IGameTelemetry>(Disposed)
        );

        _testScheduler.Start(() =>
            _pmn.GoToKnownTireState(gameTelemetryObservable)
        ).Messages.AssertEqual(
            OnNext(Subscribed + 0, GameAction.PitMenuDown),
            OnNext(Subscribed + 0, GameAction.PitMenuDown),
            OnNext(Subscribed + 0, GameAction.PitMenuRight),
            OnNext(Subscribed + 1 * TimeoutTicks, GameAction.PitMenuLeft),
            OnNext(Subscribed + 1 * TimeoutTicks, GameAction.PitMenuUp),
            OnNext(Subscribed + 1 * TimeoutTicks, GameAction.PitMenuRight),
            OnNext(Subscribed + 2 * TimeoutTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + 2 * TimeoutTicks, GameAction.PitMenuRight),
            OnNext(Subscribed + 3 * TimeoutTicks, GameAction.PitMenuLeft),
            OnNext(Subscribed + 3 * TimeoutTicks, GameAction.PitMenuUp),
            OnNext(Subscribed + 3 * TimeoutTicks, GameAction.PitMenuRight),
            OnError<GameAction>(Subscribed + 4 * TimeoutTicks,
                _ => _.Message.Contains("unknown state"))
        );
    }
    
    #endregion

    #region SetTires

    [Fact]
    public void SetTiresDoesNotChangeAnythingIfNotPresentInTelemetry()
    {
        var gameTelemetryObservable = _testScheduler.CreateColdObservable(
            OnNext<IGameTelemetry>(TelemetryTicks, GameTelemetry.Empty),
            OnCompleted<IGameTelemetry>(Disposed)
        );
        var psr = new RaceDirector.PitCrew.Protocol.PitMenu(
            FuelToAddL: null,
            TireSet: 1,
            FrontTires: new PitMenuTires(
                Compound: TireCompound.Unknown,
                LeftPressureKpa: IPressure.FromPsi(2.2).Kpa,
                RightPressureKpa: IPressure.FromPsi(3.2).Kpa
            ),
            RearTires: new PitMenuTires(
                Compound: TireCompound.Unknown,
                LeftPressureKpa: IPressure.FromPsi(4.2).Kpa,
                RightPressureKpa: IPressure.FromPsi(5.2).Kpa
            )
        );

        _testScheduler.Start(() =>
            _pmn.SetTires(psr, gameTelemetryObservable, NullLogger.Instance)
        ).Messages.AssertEqual(
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnCompleted<GameAction>(Subscribed + TelemetryTicks)
        );
    }
    
    [Fact]
    public void SetTiresDoesNotChangeTiresIfBothAxlesAreNull()
    {
        var gameTelemetryObservable = _testScheduler.CreateColdObservable(
            OnCompleted<IGameTelemetry>(Disposed)
        );
        var psr = new RaceDirector.PitCrew.Protocol.PitMenu(
            FuelToAddL: null,
            TireSet: null,
            FrontTires: null,
            RearTires: null
        );

        _testScheduler.Start(() =>
            _pmn.SetTires(psr, gameTelemetryObservable, NullLogger.Instance)
        ).Messages.AssertEqual(
            OnNext(Subscribed, GameAction.PitMenuUp),
            OnNext(Subscribed, GameAction.PitMenuRight),
            OnCompleted<GameAction>(Subscribed)
        );
    }
    
    [Fact]
    public void SetTiresDoesNotChangeAnythingIfNotRequested()
    {
        var gameTelemetryObservable = _testScheduler.CreateColdObservable(
            OnNext(TelemetryTicks, AnyTireTelemetry()),
            OnCompleted<IGameTelemetry>(Disposed)
        );
        var psr = new RaceDirector.PitCrew.Protocol.PitMenu(
            FuelToAddL: null,
            TireSet: null,
            FrontTires: new PitMenuTires(
                Compound: TireCompound.Unknown,
                LeftPressureKpa: null,
                RightPressureKpa: null
            ),
            RearTires: new PitMenuTires(
                Compound: TireCompound.Unknown,
                LeftPressureKpa: null,
                RightPressureKpa: null
            )
        );

        _testScheduler.Start(() =>
            _pmn.SetTires(psr, gameTelemetryObservable, NullLogger.Instance)
        ).Messages.AssertEqual(
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnCompleted<GameAction>(Subscribed + TelemetryTicks)
        );
    }

    [Fact]
    public void SetTiresErrorsIfDifferentCompoundsRequested()
    {
        var gameTelemetryObservable = _testScheduler.CreateColdObservable(
            OnNext(TelemetryTicks, AnyTireTelemetry()),
            OnCompleted<IGameTelemetry>(Disposed)
        );
        var psr = new RaceDirector.PitCrew.Protocol.PitMenu(
            FuelToAddL: null,
            TireSet: null,
            FrontTires: new PitMenuTires(
                Compound: TireCompound.Dry,
                LeftPressureKpa: null,
                RightPressureKpa: null
            ),
            RearTires: new PitMenuTires(
                Compound: TireCompound.Wet,
                LeftPressureKpa: null,
                RightPressureKpa: null
            )
        );

        _testScheduler.Start(() =>
            _pmn.SetTires(psr, gameTelemetryObservable, NullLogger.Instance)
        ).Messages.AssertEqual(
            OnError<GameAction>(Subscribed, _ => _.Message.Contains("compound per axle"))
        );
    }
    
    [Fact]
    public void SetTiresErrorsIfOnlyOneAxleRequested()
    {
        var gameTelemetryObservable = _testScheduler.CreateColdObservable(
            OnCompleted<IGameTelemetry>(Disposed)
        );
        var psr = new RaceDirector.PitCrew.Protocol.PitMenu(
            FuelToAddL: null,
            TireSet: null,
            FrontTires: null,
            RearTires: new PitMenuTires(
                Compound: TireCompound.Unknown,
                LeftPressureKpa: null,
                RightPressureKpa: null
            )
        );

        _testScheduler.Start(() =>
            _pmn.SetTires(psr, gameTelemetryObservable, NullLogger.Instance)
        ).Messages.AssertEqual(
            OnError<GameAction>(Subscribed, _ => _.Message.Contains("one axle"))
        );
    }
    
    [Fact]
    public void SetTiresAppliesWetCompound()
    {
        var gameTelemetryObservable = _testScheduler.CreateColdObservable(
            OnNext(TelemetryTicks, AnyTireTelemetry()),
            OnCompleted<IGameTelemetry>(Disposed)
        );
        var psr = new RaceDirector.PitCrew.Protocol.PitMenu(
            FuelToAddL: null,
            TireSet: null,
            FrontTires: new PitMenuTires(
                Compound: TireCompound.Wet,
                LeftPressureKpa: null,
                RightPressureKpa: null
            ),
            RearTires: new PitMenuTires(
                Compound: TireCompound.Wet,
                LeftPressureKpa: null,
                RightPressureKpa: null
            )
        );

        _testScheduler.Start(() =>
            _pmn.SetTires(psr, gameTelemetryObservable, NullLogger.Instance)
        ).Messages.AssertEqual(
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuRight),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnCompleted<GameAction>(Subscribed + TelemetryTicks)
        );
    }
    
    [Fact]
    public void SetTiresAppliesThePressureDifference()
    {
        var gameTelemetryObservable = _testScheduler.CreateColdObservable(
            OnNext(TelemetryTicks, TelemetryWithTires(
                tireSet: 0,
                tirePressures: new [] {
                    new [] { IPressure.FromPsi(2.3), IPressure.FromPsi(3.1) },
                    new [] { IPressure.FromPsi(4.3), IPressure.FromPsi(5.1) }
                })),
            OnCompleted<IGameTelemetry>(Disposed)
        );
        var psr = new RaceDirector.PitCrew.Protocol.PitMenu(
            FuelToAddL: null,
            TireSet: 1,
            FrontTires: new PitMenuTires(
                Compound: TireCompound.Dry,
                LeftPressureKpa: IPressure.FromPsi(2.2).Kpa,
                RightPressureKpa: IPressure.FromPsi(3.2).Kpa
            ),
            RearTires: new PitMenuTires(
                Compound: TireCompound.Dry,
                LeftPressureKpa: IPressure.FromPsi(4.2).Kpa,
                RightPressureKpa: IPressure.FromPsi(5.2).Kpa
            )
        );

        _testScheduler.Start(() =>
            _pmn.SetTires(psr, gameTelemetryObservable, NullLogger.Instance)
        ).Messages.AssertEqual(
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuRight),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuLeft),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuRight),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuLeft),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuRight),
            OnCompleted<GameAction>(Subscribed + TelemetryTicks)
        );
    }

    #endregion
    
    #region Test Setup

    private const int TimeoutTicks = 13;
    private const int TelemetryTicks = 3;
    
    private readonly TestScheduler _testScheduler;
    private readonly ACCPitMenuNavigator _pmn;

    public ACCPitMenuNavigatorTest()
    {
        _testScheduler = new TestScheduler();
        _pmn = new ACCPitMenuNavigator(TimeSpan.FromTicks(TimeoutTicks), _testScheduler);
    }

    private static IGameTelemetry TelemetryWithPitFuelToAdd(ICapacity? fuelToAdd) =>
        TelemetryWithPitMenu(new PitMenu(
            FocusedItem: PitMenuFocusedItem.Unavailable,
            SelectedItems: 0,
            FuelToAdd: fuelToAdd,
            StrategyTireSet: null,
            TireSet: null,
            TirePressures: Array.Empty<IPressure[]>()
        ));
    
    private static IGameTelemetry TelemetryWithTireSet(uint? tireSet) =>
        TelemetryWithTires(tireSet, Array.Empty<IPressure[]>());

    private static IGameTelemetry TelemetryWithTires(uint? tireSet, IPressure[][] tirePressures) =>
        TelemetryWithPitMenu(new PitMenu(
            FocusedItem: PitMenuFocusedItem.Unavailable,
            SelectedItems: 0,
            FuelToAdd: null,
            StrategyTireSet: null,
            TireSet: tireSet,
            TirePressures: tirePressures
        ));
    
    private static IGameTelemetry TelemetryWithPitMenu(IPitMenu pitMenu)
    {
        var telemetryMock = new Mock<IGameTelemetry>();
        telemetryMock.SetupGet(_ => _.Player)
            .Returns(() =>
            {
                var playerMock = new Mock<IPlayer>();
                playerMock.SetupGet(_ => _.PitMenu)
                    .Returns(() => pitMenu);
                return playerMock.Object;
            });
        return telemetryMock.Object;
    }

    private static IGameTelemetry AnyTireTelemetry() =>
        TelemetryWithTires(
            tireSet: 0,
            tirePressures: new[]
            {
                new[] {IPressure.FromPsi(12), IPressure.FromPsi(34)},
                new[] {IPressure.FromPsi(56), IPressure.FromPsi(78)}
            }
        );

    #endregion
}