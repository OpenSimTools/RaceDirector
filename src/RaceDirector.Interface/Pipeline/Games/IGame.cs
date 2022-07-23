using RaceDirector.Pipeline.GameMonitor;
using RaceDirector.Pipeline.Telemetry;

namespace RaceDirector.Pipeline.Games;

public interface IGame : ITelemetryObservableFactory, IGameProcessInfo
{
}