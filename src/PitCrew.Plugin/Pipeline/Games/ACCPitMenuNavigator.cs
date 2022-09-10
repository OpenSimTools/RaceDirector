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
            .Concat(Observable.Repeat(GameAction.PitMenuRight, psr.FuelToAdd));
}