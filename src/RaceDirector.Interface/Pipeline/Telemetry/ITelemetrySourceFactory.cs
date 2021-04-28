
using RaceDirector.Pipeline.Games;
using System.Threading.Tasks.Dataflow;

namespace RaceDirector.Pipeline.Telemetry
{
    public interface ITelemetrySourceFactory : IGameInfo
    {
        ISourceBlock<V0.ILiveTelemetry> CreateTelemetrySource();
    }
}
