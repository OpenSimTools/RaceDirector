using RaceDirector.Pipeline.Telemetry.Physics;
using RaceDirector.Pipeline.Telemetry.V0;
using RaceDirector.PitCrew.Protocol;
using RaceDirector.Remote.Networking;
using RaceDirector.Remote.Networking.Client;

namespace RaceDirector.PitCrew.Pipeline;

public class PitCrewClient : WsClient<IGameTelemetry, IPitStrategyRequest?>
{
    public PitCrewClient(string serverUrl, TimeSpan throttling) : base(serverUrl, PitCrewCodec, throttling)
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
                TirePressuresKpa: ToTyrePressureValues(gt.Player.Tires),
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

    private static TireValues<double>? ToTyrePressureValues(ITire[][] playerTires)
    {
        if (!IsTwoByTwo(playerTires))
            return null;
        var fronts = playerTires[0];
        var rears = playerTires[1];
        return new TireValues<double>(
            fronts[0].Pressure.Kpa,
            fronts[1].Pressure.Kpa,
            rears[0].Pressure.Kpa,
            rears[1].Pressure.Kpa
        );
    }

    private static PitMenuTires? ToPitMenuTires(IPressure[][] tirePressures, int index)
    {
        if (!IsTwoByTwo(tirePressures))
            return null;
        var axle = tirePressures[index];
        return new PitMenuTires(axle[0].Kpa, axle[1].Kpa);
    }

    private static bool IsTwoByTwo<T>(T[][] matrix) =>
        matrix.Length == 2 && matrix[0].Length == 2 && matrix[1].Length == 2;
}