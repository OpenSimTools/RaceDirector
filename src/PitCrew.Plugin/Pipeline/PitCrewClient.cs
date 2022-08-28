using RaceDirector.Pipeline.Telemetry.V0;
using RaceDirector.PitCrew.Protocol;
using RaceDirector.Remote.Networking;
using RaceDirector.Remote.Networking.Client;

namespace RaceDirector.PitCrew.Pipeline;

public class PitCrewClient : WsClient<IGameTelemetry, IPitStrategyRequest?>
{
    public PitCrewClient(string serverUrl) : base(serverUrl, PitCrewCodec)
    {
    }

    private static readonly Codec<IGameTelemetry, IPitStrategyRequest?> PitCrewCodec = new()
    {
        Encode = Codec.JsonEncode<PitCrewMessage>().IgnoreNull().Select<IGameTelemetry, PitCrewMessage?>(TransformTelemetry),
        Decode = Codec.JsonDecode<PitCrewMessage>().IgnoreErrors().Select<PitCrewMessage?, IPitStrategyRequest?>(m => m?.PitStrategyRequest)
    };

    private static PitCrewMessage? TransformTelemetry(IGameTelemetry gt) =>
        gt.Player is null ? null : new PitCrewMessage(
            Telemetry: new Telemetry(
                new Fuel(gt.Player?.Fuel.Left.L ?? -1.0), // TODO don't send if no player
                new PitMenu(null)
            ),
            PitStrategyRequest: null
        );
}