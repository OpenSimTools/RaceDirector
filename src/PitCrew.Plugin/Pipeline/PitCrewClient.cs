using RaceDirector.Pipeline.Telemetry.V0;
using RaceDirector.PitCrew.Protocol;
using RaceDirector.Remote.Networking;
using RaceDirector.Remote.Networking.Client;

namespace RaceDirector.PitCrew.Pipeline;

public class PitCrewClient : WsClient<IGameTelemetry, IRequestPitStrategy>
{
    public PitCrewClient(string serverUrl) : base(serverUrl, PitCrewCodec)
    {
    }

    private static readonly Codec<IGameTelemetry, IRequestPitStrategy> PitCrewCodec = new() {

        Encode = Codec.JsonEncode<Telemetry>().Select<IGameTelemetry, Telemetry>(TransformTelemetry), 
        Decode = Codec.JsonDecode<RequestPitStrategy>()
    };

    private static Telemetry TransformTelemetry(IGameTelemetry gt) => new
        (
            new Fuel(gt.Player?.Fuel.Left.L ?? -1.0), // TODO don't send if no player
            new PitMenu(null)
        );

}