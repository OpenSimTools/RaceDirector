using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using RaceDirector.DeviceIO.Pipeline;
using RaceDirector.Pipeline.Games;
using RaceDirector.Pipeline.Telemetry.Physics;
using RaceDirector.Pipeline.Telemetry.V0;
using RaceDirector.PitCrew.Protocol;

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

    public IObservable<GameAction> SetStrategy(IPitStrategyRequest psr,
        IObservable<IGameTelemetry> gto, ILogger logger) =>
        GoToFuel(gto)
            .Concat(SetFuel(psr.FuelToAddL, gto, logger))
            .Concat(GoToKnownTireState(gto))
            .Concat(SetTires(psr.TireSet, psr.FrontTires?.LeftPressureKpa, psr.FrontTires?.RightPressureKpa,
                psr.RearTires?.LeftPressureKpa, psr.RearTires?.RightPressureKpa, gto, logger))
            .Append(GameAction.PitMenuDown) // Leave selection to brakes entry
            .Catch(Observable.Return(GameAction.PitMenuOpen)); // Leave selection to top on error

    public IObservable<GameAction> GoToFuel(IObservable<IGameTelemetry> gto)
    {
        var fuelEntry = gto.Select(_ => _.Player?.PitMenu.FuelToAdd?.L);

        return ChangeMonitor<double?, GameAction>.MonitorChanges(fuelEntry, _timeout, _schedulerOverride)
            .Send(GameAction.PitMenuDown, GameAction.PitMenuDown, GameAction.PitMenuLeft, GameAction.PitMenuLeft, GameAction.PitMenuRight)
            .OrSend(GameAction.PitMenuRight, GameAction.PitMenuUp, GameAction.PitMenuLeft, GameAction.PitMenuLeft, GameAction.PitMenuRight)
            .OrEndWith(new Exception("Pit menu in unknown state"));
    }

    public IObservable<GameAction> SetFuel(double? requestedFuelToAdd,
        IObservable<IGameTelemetry> gto, ILogger logger) =>
        gto.Take(1).SelectMany(gt =>
        {
            var fuelInMenu = gt.Player?.PitMenu.FuelToAdd?.L;
            return AdjustValue("Fuel", fuelInMenu, requestedFuelToAdd, logger);
        });

    public IObservable<GameAction> GoToKnownTireState(IObservable<IGameTelemetry> gto)
    {
        var tireSetEntry = gto.Select(_ => _.Player?.PitMenu.TireSet);

        return ChangeMonitor<uint?, GameAction>.MonitorChanges(tireSetEntry, _timeout, _schedulerOverride)
            .Send(GameAction.PitMenuDown, GameAction.PitMenuDown, GameAction.PitMenuRight)
            .OrSend(GameAction.PitMenuLeft, GameAction.PitMenuUp, GameAction.PitMenuRight)
            .OrSend(GameAction.PitMenuDown, GameAction.PitMenuRight)
            .OrSend(GameAction.PitMenuLeft, GameAction.PitMenuUp, GameAction.PitMenuRight)
            .OrEndWith(new Exception("Pit menu in unknown state"));
    }
    
    public IObservable<GameAction> SetTires(uint? psrTireSet, double? psrFrontLeftKpa, double? psrFrontRightKpa,
        double? psrRearLeftKpa, double? psrRearRightKpa, IObservable<IGameTelemetry> gto, ILogger logger) =>
        gto.Take(1).SelectMany(gt =>
        {
            var pitMenu = gt.Player?.PitMenu;
            IObservable<GameAction> AdjustPressure(string itemName, int frontRear, int leftRight, double? requestedKpa)
            {
                var axle = pitMenu?.TirePressures.ElementAtOrDefault(frontRear);
                var menuPsi = axle?.ElementAtOrDefault(leftRight)?.Psi;
                double? requestedPsi = requestedKpa is null ? null : IPressure.FromKpa((double) requestedKpa).Psi;
                return AdjustValue(itemName, menuPsi * 10.0, requestedPsi * 10.0, logger);
            }
            
            return AdjustValue("Tire Set", pitMenu?.TireSet, psrTireSet, logger)
                .Append(GameAction.PitMenuDown)
                // TODO Set tire compound
                .Append(GameAction.PitMenuDown)
                .Append(GameAction.PitMenuDown)
                .Concat(AdjustPressure("Front Left", 0, 0, psrFrontLeftKpa))
                .Append(GameAction.PitMenuDown)
                .Concat(AdjustPressure("Front Right", 0, 1, psrFrontRightKpa))
                .Append(GameAction.PitMenuDown)
                .Concat(AdjustPressure("Rear Left", 1, 0, psrRearLeftKpa))
                .Append(GameAction.PitMenuDown)
                .Concat(AdjustPressure("Rear Right", 1, 1, psrRearRightKpa));
        });

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