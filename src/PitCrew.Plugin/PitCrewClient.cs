using RaceDirector.Pipeline.Telemetry.V0;
using RaceDirector.PitCrew.Protocol;
using RaceDirector.Remote.Networking;
using RaceDirector.Remote.Networking.Client;

namespace RaceDirector.PitCrew;

public class PitCrewClient : WsClient<IGameTelemetry, Nothing>
{
    public PitCrewClient(string serverUrl) : base(serverUrl, Codec.EncodeOnly(GameTelemetryEncode))
    {
    }

    private static readonly Encode<IGameTelemetry> GameTelemetryEncode = gt => TelemetryEncoder(TransformTelemetry(gt));

    private static readonly Encode<Telemetry> TelemetryEncoder = Codec.JsonEncode<Telemetry>();

    private static Telemetry TransformTelemetry(IGameTelemetry gt) => new(new Fuel(gt.Player?.Fuel.Left));
}