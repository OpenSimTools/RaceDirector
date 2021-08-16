using FsCheck;
using System;

namespace HUD.Tests.TestUtils.Arbitraries
{
    public class TelemetryArbitraries
    {
        public static Arbitrary<Double> LimitedDouble() =>
            Arb.Default.Float().Filter(d => d < Int32.MaxValue && d > Int32.MinValue);

        public static Arbitrary<RaceDirector.Pipeline.Telemetry.GameTelemetry> GameTelemetry() =>
            Arb.Default.Derive<RaceDirector.Pipeline.Telemetry.GameTelemetry>();

        public static Arbitrary<RaceDirector.Pipeline.Telemetry.Event> Event() =>
            Arb.Default.Derive<RaceDirector.Pipeline.Telemetry.Event>();

        public static Arbitrary<RaceDirector.Pipeline.Telemetry.TrackLayout> TrackLayout() =>
            Arb.Default.Derive<RaceDirector.Pipeline.Telemetry.TrackLayout>();

        public static Arbitrary<RaceDirector.Pipeline.Telemetry.Session> Session() =>
            Arb.Default.Derive<RaceDirector.Pipeline.Telemetry.Session>();

        public static Arbitrary<RaceDirector.Pipeline.Telemetry.SessionRequirements> SessionRequirements() =>
            Arb.Default.Derive<RaceDirector.Pipeline.Telemetry.SessionRequirements>();

        public static Arbitrary<RaceDirector.Pipeline.Telemetry.StartLights> StartLights() =>
            Arb.Default.Derive<RaceDirector.Pipeline.Telemetry.StartLights>();

        public static Arbitrary<RaceDirector.Pipeline.Telemetry.Vehicle> Vehicle() =>
            Arb.Default.Derive<RaceDirector.Pipeline.Telemetry.Vehicle>();

        public static Arbitrary<RaceDirector.Pipeline.Telemetry.Player> Player() =>
            Arb.Default.Derive<RaceDirector.Pipeline.Telemetry.Player>();

        public static Arbitrary<RaceDirector.Pipeline.Telemetry.RawInputs> RawInputs() =>
            Arb.Default.Derive<RaceDirector.Pipeline.Telemetry.RawInputs>();

        public static Arbitrary<RaceDirector.Pipeline.Telemetry.DrivingAids> DrivingAids() =>
            Arb.Default.Derive<RaceDirector.Pipeline.Telemetry.DrivingAids>();

        public static Arbitrary<RaceDirector.Pipeline.Telemetry.VehicleSettings> VehicleSettings() =>
            Arb.Default.Derive<RaceDirector.Pipeline.Telemetry.VehicleSettings>();

        public static Arbitrary<RaceDirector.Pipeline.Telemetry.VehicleDamage> VehicleDamage() =>
            Arb.Default.Derive<RaceDirector.Pipeline.Telemetry.VehicleDamage>();

        public static Arbitrary<RaceDirector.Pipeline.Telemetry.Tyre> Tyre() =>
            Arb.Default.Derive<RaceDirector.Pipeline.Telemetry.Tyre>();

        public static Arbitrary<RaceDirector.Pipeline.Telemetry.TemperaturesSingle> TemperaturesSingle() =>
            Arb.Default.Derive<RaceDirector.Pipeline.Telemetry.TemperaturesSingle>();

        public static Arbitrary<RaceDirector.Pipeline.Telemetry.TemperaturesMatrix> TemperaturesMatrix() =>
            Arb.Default.Derive<RaceDirector.Pipeline.Telemetry.TemperaturesMatrix>();

        public static Arbitrary<RaceDirector.Pipeline.Telemetry.Fuel> Fuel() =>
            Arb.Default.Derive<RaceDirector.Pipeline.Telemetry.Fuel>();

        public static Arbitrary<RaceDirector.Pipeline.Telemetry.Engine> Engine() =>
            Arb.Default.Derive<RaceDirector.Pipeline.Telemetry.Engine>();

        public static Arbitrary<RaceDirector.Pipeline.Telemetry.PlayerPit> PlayerPit() =>
            Arb.Default.Derive<RaceDirector.Pipeline.Telemetry.PlayerPit>();

        public static Arbitrary<RaceDirector.Pipeline.Telemetry.Physics.IDistance> IDistance() =>
            Gen.OneOf(
                Arb.Generate<Double>().Select(meters => RaceDirector.Pipeline.Telemetry.Physics.IDistance.FromM(meters)),
                Arb.Generate<Double>().Select(meters => RaceDirector.Pipeline.Telemetry.Physics.IDistance.FromKm(meters)),
                Arb.Generate<Double>().Select(meters => RaceDirector.Pipeline.Telemetry.Physics.IDistance.FromMi(meters))
            ).ToArbitrary();

        public static Arbitrary<RaceDirector.Pipeline.Telemetry.IFraction<RaceDirector.Pipeline.Telemetry.Physics.IDistance>> IFractionOfIDistance() => (
            from total in Arb.Generate<RaceDirector.Pipeline.Telemetry.Physics.IDistance>()
            from fraction in Arb.Generate<Double>()
            where fraction >= 0 && fraction <= 1 && total.M > 0
            select RaceDirector.Pipeline.Telemetry.IFraction.Of(total, fraction)
            ).ToArbitrary();

        public static Arbitrary<RaceDirector.Pipeline.Telemetry.V0.ISessionDuration> ISessionDuration() =>
             Gen.OneOf(
                DeriveGen<RaceDirector.Pipeline.Telemetry.V0.ISessionDuration.LapsDuration, RaceDirector.Pipeline.Telemetry.V0.ISessionDuration>(),
                DeriveGen<RaceDirector.Pipeline.Telemetry.V0.ISessionDuration.TimeDuration, RaceDirector.Pipeline.Telemetry.V0.ISessionDuration>(),
                DeriveGen<RaceDirector.Pipeline.Telemetry.V0.ISessionDuration.TimePlusLapsDuration, RaceDirector.Pipeline.Telemetry.V0.ISessionDuration>()
            ).ToArbitrary();

        public static Arbitrary<RaceDirector.Pipeline.Telemetry.LapTime> LapTime() =>
            Arb.Default.Derive<RaceDirector.Pipeline.Telemetry.LapTime>();

        public static Arbitrary<RaceDirector.Pipeline.Telemetry.Sectors> Sectors() =>
            Arb.Default.Derive<RaceDirector.Pipeline.Telemetry.Sectors>();

        public static Arbitrary<RaceDirector.Pipeline.Telemetry.BoundedValue<UInt32>> BoundedValueOfUInt32() =>
            Arb.Default.Derive<RaceDirector.Pipeline.Telemetry.BoundedValue<UInt32>>();


        public static Gen<I> DeriveGen<C, I>() where C : I => Arb.Default.Derive<C>().Generator.Select(c => (I)c);
    }
}
