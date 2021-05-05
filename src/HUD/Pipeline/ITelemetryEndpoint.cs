using NetCoreServer;
using ILiveTelemetry = RaceDirector.Pipeline.Telemetry.V0.ILiveTelemetry;

namespace RaceDirector.Plugin.HUD.Pipeline
{
    public interface ITelemetryEndpoint
    {
        bool Matches(HttpRequest request);

        object Transform(ILiveTelemetry telemetry);
    }
}
