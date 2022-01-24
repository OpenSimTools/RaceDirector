using System;

namespace RaceDirector.Pipeline.Telemetry.Physics
{
    public record Vector3<T>(T X, T Y, T Z);

    public interface IDistance
    {
        private static double s_KmToMRatio = 1000d;
        private static double s_MiToMRatio = 1609.344d;

        /// <summary>
        /// Distance in m
        /// </summary>
        double M { get; }

        /// <summary>
        /// Distance in Km
        /// </summary>
        double Km { get; }

        /// <summary>
        /// Distance in mi
        /// </summary>
        double Mi { get; }

        IDistance Mul(double factor);
        IDistance Div(double factor);
        double Div(IDistance other);

        public static IDistance operator *(IDistance distance, double factor) => distance.Mul(factor);
        public static IDistance operator /(IDistance distance, double factor) => distance.Div(factor);
        public static double operator /(IDistance distance, IDistance other) => distance.Div(other);

        public static IDistance FromM(double M) => new DistanceM(M);
        public static IDistance FromKm(double Km) => new DistanceKm(Km);
        public static IDistance FromMi(double Mi) => new DistanceMi(Mi);

        private record DistanceM(double M) : IDistance
        {
            private static double s_MToKmRatio = 1d / s_KmToMRatio;
            private static double s_MToMiRatio = 1d / s_MiToMRatio;

            public double Km => M * s_MToKmRatio;
            public double Mi => M * s_MToMiRatio;

            public IDistance Mul(double factor) => new DistanceM(M * factor);
            public IDistance Div(double factor) => new DistanceM(M / factor);
            public double Div(IDistance other) => M / other.M;
        }

        private record DistanceKm(double Km) : IDistance
        {
            private static double s_KmToMiRatio = s_KmToMRatio / s_MiToMRatio;

            public double M => Km * s_KmToMRatio;
            public double Mi => Km * s_KmToMiRatio;

            public IDistance Mul(double factor) => new DistanceKm(Km * factor);
            public IDistance Div(double factor) => new DistanceKm(Km / factor);
            public double Div(IDistance other) => Km / other.Km;
        }

        private record DistanceMi(double Mi) : IDistance
        {
            private static double s_MiToKmRatio = s_MiToMRatio / s_KmToMRatio;

            public double M => Mi * s_MiToMRatio;
            public double Km => Mi * s_MiToKmRatio;

            public IDistance Mul(double factor) => new DistanceMi(Mi * factor);
            public IDistance Div(double factor) => new DistanceMi(Mi / factor);
            public double Div(IDistance other) => Mi / other.Mi;
        }
    }

    public interface ISpeed
    {
        double MPS { get; }
        double KmPH { get; }
        double MiPH { get; }

        static ISpeed FromMPS(double MPS) => new Speed(IDistance.FromM(MPS), TimeSpan.FromSeconds(1));
        static ISpeed FromKmPH(double KmPH) => new Speed(IDistance.FromKm(KmPH), TimeSpan.FromHours(1));
        static ISpeed FromMiPH(double MiPH) => new Speed(IDistance.FromMi(MiPH), TimeSpan.FromHours(1));

        private class Speed : ISpeed
        {
            public Speed(IDistance Distance, TimeSpan Time)
            {
                this.Distance = Distance;
                this.Time = Time;
            }

            private readonly IDistance Distance;
            private readonly TimeSpan Time;

            public double MPS => Distance.M / Time.TotalSeconds;
            public double KmPH => Distance.Km / Time.TotalHours;
            public double MiPH => Distance.Mi / Time.TotalHours;
        }
    }

    public interface IAcceleration
    {
        private static double s_GToMPS2 = 9.80665d;
        private static double s_ApproxGToMPS2 = 9.81d;

        double MPS2 { get; }
        double G { get; }
        double ApproxG { get; }

        static IAcceleration FromMPS2(double MPS2) => new AccelerationMPS2(MPS2);
        static IAcceleration FromG(double G) => new AccelerationG(G);
        static IAcceleration FromApproxG(double G) => new AccelerationApproxG(G);

        private record AccelerationMPS2(double MPS2) : IAcceleration
        {
            private static double s_MPS2ToG = 1d / s_GToMPS2;
            private static double s_MPS2ToApproxG = 1d / s_ApproxGToMPS2;

            public double G => MPS2 * s_MPS2ToG;
            public double ApproxG => MPS2 * s_MPS2ToApproxG;
        }

        private record AccelerationG(double G) : IAcceleration
        {
            private static double s_GToApproxG = s_ApproxGToMPS2 / s_GToMPS2;

            public double MPS2 => G * s_GToMPS2;
            public double ApproxG => G * s_GToApproxG;
        }

        private record AccelerationApproxG(double ApproxG) : IAcceleration
        {
            private static double s_ApproxGToG = s_GToMPS2 / s_ApproxGToMPS2;

            public double MPS2 => G * s_GToMPS2;
            public double G => ApproxG * s_ApproxGToG;
        }
    }

    public interface IAngle
    {
        private static double s_RevToRadRatio = 2d * Math.PI;
        private static double s_RevToDegRatio = 360d;

        /// <summary>
        /// Angle in degrees.
        /// </summary>
        double Deg { get; }

        /// <summary>
        /// Angle in radians.
        /// </summary>
        double Rad { get; }

        /// <summary>
        /// Complete rotations (AKA revolutions or turns).
        /// </summary>
        double Rev { get; }

        static IAngle FromDeg(double Deg) => new AngleDegrees(Deg);
        static IAngle FromRad(double Rad) => new AngleRadians(Rad);
        static IAngle FromRev(double Rev) => new AngleRevolutions(Rev);

        private record AngleRadians(double Rad) : IAngle
        {
            private static double s_DegRatio = s_RevToDegRatio / s_RevToRadRatio;
            private static double s_RevRatio = 1d / s_RevToRadRatio;

            public double Deg => Rad * s_DegRatio;
            public double Rev => Rad * s_RevRatio;
        }

        private record AngleDegrees(double Deg) : IAngle
        {
            private static double s_RadRatio = s_RevToRadRatio / s_RevToDegRatio;
            private static double s_RevRatio = 1d / s_RevToDegRatio;

            public double Rad => Deg * s_RadRatio;
            public double Rev => Deg * s_RevRatio;
        }

        private record AngleRevolutions(double Rev) : IAngle
        {
            public double Deg => Rev * s_RevToDegRatio;
            public double Rad => Rev * s_RevToRadRatio;
        }
    }

    public interface IAngularSpeed
    {
        double RadPS { get; }
        double RevPS { get; }

        static IAngularSpeed FromRadPS(double RadPS) => new AngularSpeed(IAngle.FromRad(RadPS));
        static IAngularSpeed FromRevPS(double RevPS) => new AngularSpeed(IAngle.FromRev(RevPS));

        private class AngularSpeed : IAngularSpeed
        {
            public AngularSpeed(IAngle Angle)
            {
                this.Angle = Angle;
            }

            private readonly IAngle Angle;

            public double RadPS => Angle.Rad;
            public double RevPS => Angle.Rev;
        }
    }

    public record Orientation(
        IAngle Yaw,
        IAngle Pitch,
        IAngle Roll
    );

    public interface ITemperature
    {
        private static double s_CToKDiff = 273.15d;
        private static double s_CToFDiff = 32d;
        private static double s_KCToFRatio = 9d / 5d;

        double K { get; }
        double C { get; }
        double F { get; }

        static ITemperature FromK(double K) => new TemperatureKelvin(K);
        static ITemperature FromC(double C) => new TemperatureCelsius(C);
        static ITemperature FromF(double F) => new TemperatureFahrenheit(F);

        public record TemperatureKelvin(double K) : ITemperature
        {
            public double C => K - s_CToKDiff;
            public double F => C * s_KCToFRatio + s_CToFDiff;
        }

        public record TemperatureCelsius(double C) : ITemperature
        {
            public double K => C + s_CToKDiff;
            public double F => C * s_KCToFRatio + s_CToFDiff;
        }

        public record TemperatureFahrenheit(double F) : ITemperature
        {
            private static double s_FToKCRatio = 1 / s_KCToFRatio;

            public double K => C + s_CToKDiff;
            public double C => (F - s_CToFDiff) * s_FToKCRatio;
        }
    }
}