using System;

namespace RaceDirector.Pipeline.Telemetry.Physics
{
    public record Vector3<T>(T X, T Y, T Z);

    public interface IDistance
    {
        private static Double s_KmToMRatio = 1000d;
        private static Double s_MiToMRatio = 1609.344d;

        /// <summary>
        /// Distance in m
        /// </summary>
        Double M { get; }

        /// <summary>
        /// Distance in Km
        /// </summary>
        Double Km { get; }

        /// <summary>
        /// Distance in mi
        /// </summary>
        Double Mi { get; }

        IDistance Mul(Double factor);

        public static IDistance operator *(IDistance distance, Double factor) => distance.Mul(factor);

        public static IDistance FromM(Double M) => new DistanceM(M);
        public static IDistance FromKm(Double Km) => new DistanceKm(Km);
        public static IDistance FromMi(Double Mi) => new DistanceMi(Mi);

        private record DistanceM(Double M) : IDistance
        {
            private static Double s_MToKmRatio = 1d / s_KmToMRatio;
            private static Double s_MToMiRatio = 1d / s_MiToMRatio;

            public Double Km => M * s_MToKmRatio;
            public Double Mi => M * s_MToMiRatio;

            public IDistance Mul(double factor) => new DistanceM(M * factor);
        }

        private record DistanceKm(Double Km) : IDistance
        {
            private static Double s_KmToMiRatio = s_KmToMRatio / s_MiToMRatio;

            public Double M => Km * s_KmToMRatio;
            public Double Mi => Km * s_KmToMiRatio;

            public IDistance Mul(double factor) => new DistanceKm(Km * factor);
        }

        private record DistanceMi(Double Mi) : IDistance
        {
            private static Double s_MiToKmRatio = s_MiToMRatio / s_KmToMRatio;

            public Double M => Mi * s_MiToMRatio;
            public Double Km => Mi * s_MiToKmRatio;

            public IDistance Mul(double factor) => new DistanceMi(Mi * factor);
        }
    }

    public interface ISpeed
    {
        Double MPS { get; }
        Double KmPH { get; }
        Double MiPH { get; }

        static ISpeed FromMPS(Double MPS) => new Speed(IDistance.FromM(MPS), TimeSpan.FromSeconds(1));
        static ISpeed FromKmPH(Double KmPH) => new Speed(IDistance.FromKm(KmPH), TimeSpan.FromHours(1));
        static ISpeed FromMiPH(Double MiPH) => new Speed(IDistance.FromMi(MiPH), TimeSpan.FromHours(1));

        private class Speed : ISpeed
        {
            public Speed(IDistance Distance, TimeSpan Time)
            {
                this.Distance = Distance;
                this.Time = Time;
            }

            private readonly IDistance Distance;
            private readonly TimeSpan Time;

            public Double MPS => Distance.M / Time.TotalSeconds;
            public Double KmPH => Distance.Km / Time.TotalHours;
            public Double MiPH => Distance.Mi / Time.TotalHours;
        }
    }

    public interface IAcceleration
    {
        private static Double s_GToMPS2 = 9.80665d;

        Double MPS2 { get; }
        Double G { get; }

        static IAcceleration FromMPS2(Double MPS2) => new AccelerationMPS2(MPS2);
        static IAcceleration FromG(Double G) => new AccelerationG(G);

        private record AccelerationMPS2(Double MPS2) : IAcceleration
        {
            private static Double s_MPS2ToG = 1d / s_GToMPS2;

            public Double G => MPS2 * s_MPS2ToG;
        }

        private record AccelerationG(Double G) : IAcceleration
        {
            public Double MPS2 => G * s_GToMPS2;
        }
    }

    public interface IAngle
    {
        private static Double s_RevToRadRatio = 2d * Math.PI;
        private static Double s_RevToDegRatio = 360d;

        /// <summary>
        /// Angle in degrees.
        /// </summary>
        Double Deg { get; }

        /// <summary>
        /// Angle in radians.
        /// </summary>
        Double Rad { get; }

        /// <summary>
        /// Complete rotations (AKA revolutions or turns).
        /// </summary>
        Double Rev { get; }

        static IAngle FromDeg(Double Deg) => new AngleDegrees(Deg);
        static IAngle FromRad(Double Rad) => new AngleRadians(Rad);
        static IAngle FromRev(Double Rev) => new AngleRevolutions(Rev);

        private record AngleRadians(Double Rad) : IAngle
        {
            private static Double s_DegRatio = s_RevToDegRatio / s_RevToRadRatio;
            private static Double s_RevRatio = 1d / s_RevToRadRatio;

            public Double Deg => Rad * s_DegRatio;
            public Double Rev => Rad * s_RevRatio;
        }

        private record AngleDegrees(Double Deg) : IAngle
        {
            private static Double s_RadRatio = s_RevToRadRatio / s_RevToDegRatio;
            private static Double s_RevRatio = 1d / s_RevToDegRatio;

            public Double Rad => Deg * s_RadRatio;
            public Double Rev => Deg * s_RevRatio;
        }

        private record AngleRevolutions(Double Rev) : IAngle
        {
            public Double Deg => Rev * s_RevToDegRatio;
            public Double Rad => Rev * s_RevToRadRatio;
        }
    }

    public interface IAngularSpeed
    {
        Double RadPS { get; }
        Double RevPS { get; }

        static IAngularSpeed FromRadPS(Double RadPS) => new AngularSpeed(IAngle.FromRad(RadPS));
        static IAngularSpeed FromRevPS(Double RevPS) => new AngularSpeed(IAngle.FromRev(RevPS));

        private class AngularSpeed : IAngularSpeed
        {
            public AngularSpeed(IAngle Angle)
            {
                this.Angle = Angle;
            }

            private readonly IAngle Angle;

            public Double RadPS => Angle.Rad;
            public Double RevPS => Angle.Rev;
        }
    }

    public record Orientation(
        IAngle Yaw,
        IAngle Pitch,
        IAngle Roll
    );

    public interface ITemperature
    {
        private static Double s_CToKDiff = 273.15d;
        private static Double s_CToFDiff = 32d;
        private static Double s_KCToFRatio = 9d / 5d;

        Double K { get; }
        Double C { get; }
        Double F { get; }

        static ITemperature FromK(Double K) => new TemperatureKelvin(K);
        static ITemperature FromC(Double C) => new TemperatureCelsius(C);
        static ITemperature FromF(Double F) => new TemperatureFahrenheit(F);

        public record TemperatureKelvin(Double K) : ITemperature
        {
            public Double C => K - s_CToKDiff;
            public Double F => C * s_KCToFRatio + s_CToFDiff;
        }

        public record TemperatureCelsius(Double C) : ITemperature
        {
            public Double K => C + s_CToKDiff;
            public Double F => C * s_KCToFRatio + s_CToFDiff;
        }

        public record TemperatureFahrenheit(Double F) : ITemperature
        {
            private static Double s_FToKCRatio = 1 / s_KCToFRatio;

            public Double K => C + s_CToKDiff;
            public Double C => (F - s_CToFDiff) * s_FToKCRatio;
        }
    }
}