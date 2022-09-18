using RaceDirector.Pipeline.Telemetry.Physics;
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
                FuelLeftL: gt.Player.Fuel.Left.L,
                PitMenu: new PitMenu
                (
                    FuelToAddL: gt.Player.PitMenu.FuelToAdd?.L,
                    TireSet: gt.Player.PitMenu.TireSet,
                    FrontTires: ToPitMenuTires(gt.Player.PitMenu.TirePressures, 0),
                    RearTires: ToPitMenuTires(gt.Player.PitMenu.TirePressures, 1)
                )
            ),
            PitStrategyRequest: null
        );

    private static PitMenuTires? ToPitMenuTires(IPressure[][] tirePressures, int index)
    {
        if (tirePressures.Length <= index)
            return null;
        var axle = tirePressures[index];
        if (axle.Length != 2)
            return null;
        return new PitMenuTires(axle[0].Kpa, axle[1].Kpa);
    }
}