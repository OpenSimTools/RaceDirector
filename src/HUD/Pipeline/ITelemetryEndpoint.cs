namespace RaceDirector.Plugin.HUD.Pipeline
{
    public interface ITelemetryEndpoint
    {
        string Path { get; }

        void BroadcastTelemetry(RaceDirector.Pipeline.Telemetry.V0.ILiveTelemetry telemetry);
    }
}
