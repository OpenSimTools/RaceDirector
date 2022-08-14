using RaceDirector.Pipeline.Telemetry.V0;
using RaceDirector.PitCrew.Protocol;
using RaceDirector.Remote.Networking;
using RaceDirector.Remote.Networking.Client;
using RaceDirector.Remote.Networking.Codec;

namespace RaceDirector.PitCrew;

public class PitCrewClient : WsClient<IGameTelemetry, Nothing>
{
    private static readonly IEncoder<IGameTelemetry> Encoder = new JsonEncoder<Telemetry>()
            .Wrap<IGameTelemetry, Telemetry>(gt =>
                new Telemetry(new Fuel(gt?.Player?.Fuel.Left))
            );

    public PitCrewClient(string serverUrl) : base(serverUrl, Encoder.ToCodec())
    {
    }
}