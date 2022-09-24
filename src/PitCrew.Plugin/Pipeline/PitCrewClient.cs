using System.Text.Json;
using System.Text.Json.Serialization;
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

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    private static readonly Codec<IGameTelemetry, IPitStrategyRequest?> PitCrewCodec = new()
    {
        Encode = Codec.JsonEncode<PitCrewMessage>(JsonSerializerOptions).IgnoreNull().Select<IGameTelemetry, PitCrewMessage?>(TransformTelemetry),
        Decode = Codec.JsonDecode<PitCrewMessage>(JsonSerializerOptions).IgnoreErrors().Select<PitCrewMessage?, IPitStrategyRequest?>(m => m?.PitStrategyRequest)
    };

    private static PitCrewMessage? TransformTelemetry(IGameTelemetry gt) =>
        gt.Player is null ? null : new PitCrewMessage(
            Telemetry: new Telemetry(
                FuelLeftL: gt.Player.Fuel.Left.L,
                TireSet: gt.Player.TireSet,
                FrontTires: ToTelemetryTireAxle(gt.Player.Tires, 0),
                RearTires: ToTelemetryTireAxle(gt.Player.Tires, 1),
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

    private static TelemetryTireAxle? ToTelemetryTireAxle(ITire[][] playerTires, int index)
    {
        if (!IsTwoByTwo(playerTires))
            return null;
        var axle = playerTires[index];
        var left = axle[0];
        var right = axle[1];
        var compound = left.Compound != right.Compound ? TireCompound.Unknown : left.Compound;
        return new TelemetryTireAxle(
            Compound: compound,
            Left: new TelemetryTire(left.Pressure.Kpa, left.Wear),
            Right: new TelemetryTire(right.Pressure.Kpa, right.Wear)
        );
    }

    private static PitMenuTires? ToPitMenuTires(IPressure[][] tirePressures, int index)
    {
        if (!IsTwoByTwo(tirePressures))
            return null;
        var axle = tirePressures[index];
        return new PitMenuTires(
            Compound: TireCompound.Unknown,
            LeftPressureKpa: axle[0].Kpa,
            RightPressureKpa: axle[1].Kpa
        );
    }

    private static bool IsTwoByTwo<T>(T[][] matrix) =>
        matrix.Length == 2 && matrix[0].Length == 2 && matrix[1].Length == 2;
}