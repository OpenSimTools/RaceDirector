using RaceDirector.Pipeline.Telemetry.V0;
using RaceDirector.Remote.Networking;
using RaceDirector.Remote.Networking.Client;
using RaceDirector.Remote.Networking.Codec;

namespace RaceDirector.PitCrew;

public class PitCrewClient : WsClient<IGameTelemetry, Nothing>
{
    private static readonly IEncoder<IGameTelemetry> Encoder = new JsonEncoder<Protocol>()
            .Wrap<IGameTelemetry, Protocol>(gt => new Protocol(gt?.Player?.Fuel.Left));

    public PitCrewClient(string serverUrl) : base(serverUrl, Encoder.ToCodec())
    {
    }

    private record Protocol(double? LitersLeft);
}