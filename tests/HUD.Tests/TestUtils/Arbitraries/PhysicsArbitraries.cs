using FsCheck;
using System;

namespace HUD.Tests.TestUtils.Arbitraries
{
    public class PhysicsArbitraries
    {
        public static Arbitrary<RaceDirector.Pipeline.Telemetry.Physics.IDistance> IDistance() =>
            Gen.OneOf(
                Arb.Generate<Double>().Select(m => RaceDirector.Pipeline.Telemetry.Physics.IDistance.FromM(m)),
                Arb.Generate<Double>().Select(km => RaceDirector.Pipeline.Telemetry.Physics.IDistance.FromKm(km)),
                Arb.Generate<Double>().Select(mi => RaceDirector.Pipeline.Telemetry.Physics.IDistance.FromMi(mi))
            ).ToArbitrary();

        public static Arbitrary<RaceDirector.Pipeline.Telemetry.Physics.ISpeed> ISpeed() =>
            Gen.OneOf(
                Arb.Generate<Double>().Select(mps => RaceDirector.Pipeline.Telemetry.Physics.ISpeed.FromMPS(mps)),
                Arb.Generate<Double>().Select(kph => RaceDirector.Pipeline.Telemetry.Physics.ISpeed.FromKmPH(kph)),
                Arb.Generate<Double>().Select(miph => RaceDirector.Pipeline.Telemetry.Physics.ISpeed.FromMiPH(miph))
            ).ToArbitrary();

        public static Arbitrary<RaceDirector.Pipeline.Telemetry.Physics.IAngle> IAngle() =>
            Gen.OneOf(
                Arb.Generate<Double>().Select(deg => RaceDirector.Pipeline.Telemetry.Physics.IAngle.FromDeg(deg)),
                Arb.Generate<Double>().Select(rad => RaceDirector.Pipeline.Telemetry.Physics.IAngle.FromRad(rad)),
                Arb.Generate<Double>().Select(rev => RaceDirector.Pipeline.Telemetry.Physics.IAngle.FromRev(rev))
            ).ToArbitrary();

        public static Arbitrary<RaceDirector.Pipeline.Telemetry.Physics.ITemperature> ITemperature() =>
            Gen.OneOf(
                Arb.Generate<Double>().Select(c => RaceDirector.Pipeline.Telemetry.Physics.ITemperature.FromC(c)),
                Arb.Generate<Double>().Select(f => RaceDirector.Pipeline.Telemetry.Physics.ITemperature.FromF(f)),
                Arb.Generate<Double>().Select(k => RaceDirector.Pipeline.Telemetry.Physics.ITemperature.FromK(k))
            ).ToArbitrary();

        public static Arbitrary<RaceDirector.Pipeline.Telemetry.Physics.IAngularSpeed> IAngularSpeed() =>
            Gen.OneOf(
                Arb.Generate<Double>().Select(radps => RaceDirector.Pipeline.Telemetry.Physics.IAngularSpeed.FromRadPS(radps)),
                Arb.Generate<Double>().Select(revps => RaceDirector.Pipeline.Telemetry.Physics.IAngularSpeed.FromRevPS(revps))
            ).ToArbitrary();

        public static Arbitrary<RaceDirector.Pipeline.Telemetry.Physics.IAcceleration> IAcceleration() =>
            Gen.OneOf(
                Arb.Generate<Double>().Select(mps2 => RaceDirector.Pipeline.Telemetry.Physics.IAcceleration.FromMPS2(mps2)),
                Arb.Generate<Double>().Select(g => RaceDirector.Pipeline.Telemetry.Physics.IAcceleration.FromG(g))
            ).ToArbitrary();

        public static Gen<I> DeriveGen<C, I>() where C : I => Arb.Default.Derive<C>().Generator.Select(c => (I)c);
    }
}
