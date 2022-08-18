using RaceDirector.Pipeline.Telemetry.V0;
using RaceDirector.PitCrew.Protocol;
using RaceDirector.Remote.Networking;
using RaceDirector.Remote.Networking.Client;

namespace RaceDirector.PitCrew;

public class PitCrewClient : WsClient<IGameTelemetry, Nothing>
{
    public PitCrewClient(string serverUrl) : base(serverUrl, Codec.EncodeOnly(TelemetryEncoder))
    {
    }

    private static readonly Encode<IGameTelemetry> TelemetryEncoder =
        gt => Codec.JsonEncode<Telemetry>()(TransformTelemetry(gt));

    private static Telemetry TransformTelemetry(IGameTelemetry gt) => new(new Fuel(gt.Player?.Fuel.Left));
}