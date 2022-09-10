using RaceDirector.DeviceIO.Pipeline;
using RaceDirector.Pipeline.Games;
using RaceDirector.Pipeline.Telemetry.V0;
using RaceDirector.PitCrew.Protocol;

namespace RaceDirector.PitCrew.Pipeline.Games;

public interface IGamePitMenuNavigator : IGameInfo
{
    IObservable<GameAction> SetStrategy(IPitStrategyRequest request, IObservable<IGameTelemetry> gameTelemetryObservable);
}