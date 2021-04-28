using RaceDirector.Pipeline.Games;

namespace RaceDirector.Interface.Pipeline.GameMonitor
{
    public interface IGameProcessInfo : IGameInfo
    {
        string[] GameProcessNames { get; }
    }
}
