using RaceDirector.Pipeline.Games;

namespace RaceDirector.Pipeline.GameMonitor
{
    public interface IGameProcessInfo : IGameInfo
    {
        string[] GameProcessNames { get; }
    }
}
