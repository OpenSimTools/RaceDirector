using Microsoft.Extensions.Logging;
using RaceDirector.DeviceIO.Pipeline;
using RaceDirector.Pipeline.Games;
using RaceDirector.Pipeline.Telemetry.V0;
using RaceDirector.PitCrew.Protocol;

namespace RaceDirector.PitCrew.Pipeline.Games;

public interface IGamePitMenuNavigator : IGameInfo
{
    IObservable<GameAction> ApplyStrategy(IPitStrategyRequest request, IObservable<IGameTelemetry> gto, ILogger logger);
}