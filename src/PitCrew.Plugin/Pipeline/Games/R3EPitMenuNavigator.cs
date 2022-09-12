using System.Reactive.Linq;
using RaceDirector.DeviceIO.Pipeline;
using RaceDirector.Pipeline.Games;
using RaceDirector.Pipeline.Telemetry.V0;
using RaceDirector.PitCrew.Protocol;

namespace RaceDirector.PitCrew.Pipeline.Games;

public class R3EPitMenuNavigator : IGamePitMenuNavigator
{
    public string GameName => Names.R3E;

    public IObservable<GameAction> SetStrategy(IPitStrategyRequest psr,
        IObservable<IGameTelemetry> gameTelemetryObservable) =>
        // TODO SendKeys doesn't work in RaceRoom and we can't verify it until we implement an alternative
        Observable.Empty<GameAction>();
}