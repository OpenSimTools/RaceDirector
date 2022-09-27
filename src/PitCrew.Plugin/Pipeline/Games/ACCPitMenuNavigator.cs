using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using RaceDirector.DeviceIO.Pipeline;
using RaceDirector.Pipeline.Games;
using RaceDirector.Pipeline.Telemetry.Physics;
using RaceDirector.Pipeline.Telemetry.V0;
using RaceDirector.PitCrew.Protocol;
using RaceDirector.Reactive;

namespace RaceDirector.PitCrew.Pipeline.Games;

public class ACCPitMenuNavigator : IGamePitMenuNavigator
{
    public string GameName => Names.ACC;

    private readonly TimeSpan _timeout;
    private readonly IScheduler? _schedulerOverride;

    public ACCPitMenuNavigator(TimeSpan timeout, IScheduler? schedulerOverride = null)
    {
        _timeout = timeout;
        _schedulerOverride = schedulerOverride;
    }

    public IObservable<GameAction> ApplyStrategy(IPitStrategyRequest psr,
        IObservable<IGameTelemetry> gto, ILogger logger) =>
        Observable.Return(GameAction.PitMenuOpen)
            .Concat(GoToFuel(gto, logger))
            .Concat(SetFuel(psr.FuelToAddL, gto, logger))
            .Concat(GoToKnownTireState(gto, logger))
            .Concat(SetTires(psr, gto, logger))
            .Append(GameAction.PitMenuDown) // Leave selection to brakes entry
            .Catch((Exception ex) => // Leave selection to top on error
            {
                logger.LogWarning(ex, "Failed to apply pit strategy");
                return Observable.Return(GameAction.PitMenuOpen);
            });

    public IObservable<GameAction> GoToFuel(IObservable<IGameTelemetry> gto, ILogger logger)
    {
        var fuelEntry = gto.Select(_ => _.Player?.PitMenu.FuelToAdd?.L);

        return fuelEntry
            .MonitorChanges<double?, GameAction>(_timeout, _schedulerOverride, logger)
            .Produce(GameAction.PitMenuDown, GameAction.PitMenuDown, GameAction.PitMenuLeft, GameAction.PitMenuLeft, GameAction.PitMenuRight)
            .OrProduce(GameAction.PitMenuRight, GameAction.PitMenuUp, GameAction.PitMenuLeft, GameAction.PitMenuLeft, GameAction.PitMenuRight)
            .OrEndWith(new Exception("Pit menu in unknown state"));
    }

    public IObservable<GameAction> SetFuel(double? requestedFuelToAdd,
        IObservable<IGameTelemetry> gto, ILogger logger) =>
        gto.Take(1).SelectMany(gt =>
        {
            var fuelInMenu = gt.Player?.PitMenu.FuelToAdd?.L;
            return AdjustValue("Fuel", fuelInMenu, requestedFuelToAdd, logger);
        });

    /// <summary>
    /// Known tire state is tyre change selected, with dry compound and on tyre set entry.
    /// </summary>
    public IObservable<GameAction> GoToKnownTireState(IObservable<IGameTelemetry> gto, ILogger logger)
    {
        var tireSetEntry = gto.Select(_ => _.Player?.PitMenu.TireSet);

        return tireSetEntry
            .MonitorChanges<uint?, GameAction>(_timeout, _schedulerOverride)
            .Produce(GameAction.PitMenuDown, GameAction.PitMenuDown, GameAction.PitMenuRight)
            .OrProduce(GameAction.PitMenuLeft, GameAction.PitMenuUp, GameAction.PitMenuRight)
            .OrProduce(GameAction.PitMenuDown, GameAction.PitMenuRight)
            .OrProduce(GameAction.PitMenuLeft, GameAction.PitMenuUp, GameAction.PitMenuRight)
            .OrEndWith(new Exception("Pit menu in unknown state"));
    }

    public IObservable<GameAction> SetTires(IPitStrategyRequest psr, IObservable<IGameTelemetry> gto, ILogger logger)
    {
        // Close tire change menu if no tires to change
        if (psr.FrontTires is null && psr.RearTires is null)
            return Observable
                .Return(GameAction.PitMenuUp)
                .Append(GameAction.PitMenuRight);

        // Error if only one axle
        if (psr.FrontTires is null || psr.RearTires is null)
            return Observable.Throw<GameAction>(
                new Exception("Game does not support changing tires on one axle")
            );

        // Error if different compound per axle
        if (psr.FrontTires?.Compound != psr.RearTires?.Compound)
            return Observable.Throw<GameAction>(
                new Exception("Game does not support different compound per axle")
            );
        var compound = psr.FrontTires?.Compound;
        
        return gto.Take(1).SelectMany(gt =>
        {
            var pitMenu = gt.Player?.PitMenu;
            IObservable<GameAction> AdjustPressure(string itemName, int frontRear, int leftRight, double? requestedKpa)
            {
                var axle = pitMenu?.TirePressures.ElementAtOrDefault(frontRear);
                var menuPsi = axle?.ElementAtOrDefault(leftRight)?.Psi;
                double? requestedPsi = requestedKpa is null ? null : IPressure.FromKpa((double) requestedKpa).Psi;
                return AdjustValue(itemName, menuPsi * 10.0, requestedPsi * 10.0, logger);
            }
            
            return AdjustValue("Tire Set", pitMenu?.TireSet, psr.TireSet, logger)
                .Append(GameAction.PitMenuDown)
                .Concat(TireCompound.Wet == compound
                    ? Observable.Return(GameAction.PitMenuRight)
                    : Observable.Empty<GameAction>())
                .Append(GameAction.PitMenuDown)
                .Append(GameAction.PitMenuDown)
                .Concat(AdjustPressure("Front Left", 0, 0, psr.FrontTires?.LeftPressureKpa))
                .Append(GameAction.PitMenuDown)
                .Concat(AdjustPressure("Front Right", 0, 1, psr.FrontTires?.RightPressureKpa))
                .Append(GameAction.PitMenuDown)
                .Concat(AdjustPressure("Rear Left", 1, 0, psr.RearTires?.LeftPressureKpa))
                .Append(GameAction.PitMenuDown)
                .Concat(AdjustPressure("Rear Right", 1, 1, psr.RearTires?.RightPressureKpa));
        });
    }

    private IObservable<GameAction> AdjustValue(string itemName, double? valueInMenu, double? requestedValue,
        ILogger logger)
    {
        if (valueInMenu is null || requestedValue is null)
            return Observable.Empty<GameAction>();
        var difference = Convert.ToInt32(requestedValue - valueInMenu);
        logger.LogDebug("{} change: {}", itemName, difference);
        return difference >= 0
            ? Observable.Repeat(GameAction.PitMenuRight, difference)
            : Observable.Repeat(GameAction.PitMenuLeft, -difference);
    }

}