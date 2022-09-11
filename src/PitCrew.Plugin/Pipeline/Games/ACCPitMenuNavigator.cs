using System.Reactive.Linq;
using RaceDirector.DeviceIO.Pipeline;
using RaceDirector.Pipeline.Games;
using RaceDirector.Pipeline.Telemetry.V0;
using RaceDirector.PitCrew.Protocol;

namespace RaceDirector.PitCrew.Pipeline.Games;

public class ACCPitMenuNavigator : IGamePitMenuNavigator
{
    public string GameName => Names.ACC;

    public IObservable<GameAction> SetStrategy(IPitStrategyRequest psr, IObservable<IGameTelemetry> gameTelemetryObservable) =>
        new[] {GameAction.PitMenuOpen, GameAction.PitMenuDown, GameAction.PitMenuDown}
            .ToObservable()
            .Concat(SetFuel(psr.FuelToAdd, gameTelemetryObservable));

    private IObservable<GameAction> SetFuel(double requestedFuelToAdd, IObservable<IGameTelemetry> gameTelemetryObservable) =>
        gameTelemetryObservable.Take(1).SelectMany(t =>
        {
            var fuelInMenu = t.Player?.PitMenu.FuelToAdd?.L ?? requestedFuelToAdd;
            var fuelToAdd = (int)(requestedFuelToAdd - fuelInMenu);
            // TODO log
            return fuelToAdd >= 0
                ? Observable.Repeat(GameAction.PitMenuRight, fuelToAdd)
                : Observable.Repeat(GameAction.PitMenuLeft, -fuelToAdd);
        });
}