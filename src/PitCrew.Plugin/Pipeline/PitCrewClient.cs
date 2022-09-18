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
                new Fuel(gt.Player.Fuel.Left.L),
                new PitMenu
                (
                    FuelToAddL: gt.Player.PitMenu.FuelToAdd?.L,
                    TireSet: gt.Player.PitMenu.TireSet,
                    TirePressuresKpa: ToTirePressuresKpa(gt.Player.PitMenu.TirePressures)
                )
            ),
            PitStrategyRequest: null
        );

    private static TireValues<double>? ToTirePressuresKpa(IPressure[][] tirePressures)
    {
        if (tirePressures.Length != 2)
            return null;
        var fronts = tirePressures[0];
        var rears = tirePressures[1];
        if (fronts.Length != 2 || rears.Length != 2)
            return null;
        return new TireValues<double>(fronts[0].Kpa, fronts[1].Kpa, rears[0].Kpa, rears[1].Kpa);
    }
}