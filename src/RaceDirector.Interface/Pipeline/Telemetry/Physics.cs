using System;

namespace RaceDirector.Pipeline.Telemetry.Physics;

public record Vector3<T>(T X, T Y, T Z);

public interface IDistance
{
    private static readonly double KmToMRatio = 1000d;
    private static readonly double MiToMRatio = 1609.344d;

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

    public static IDistance FromM(double m) => new DistanceM(m);
    public static IDistance FromKm(double km) => new DistanceKm(km);
    public static IDistance FromMi(double mi) => new DistanceMi(mi);

    private record DistanceM(double M) : IDistance
    {
        private static readonly double MToKmRatio = 1d / KmToMRatio;
        private static readonly double MToMiRatio = 1d / MiToMRatio;

        public double Km => M * MToKmRatio;
        public double Mi => M * MToMiRatio;

        public IDistance Mul(double factor) => new DistanceM(M * factor);
        public IDistance Div(double factor) => new DistanceM(M / factor);
        public double Div(IDistance other) => M / other.M;
    }

    private record DistanceKm(double Km) : IDistance
    {
        private static readonly double KmToMiRatio = KmToMRatio / MiToMRatio;

        public double M => Km * KmToMRatio;
        public double Mi => Km * KmToMiRatio;

        public IDistance Mul(double factor) => new DistanceKm(Km * factor);
        public IDistance Div(double factor) => new DistanceKm(Km / factor);
        public double Div(IDistance other) => Km / other.Km;
    }

    private record DistanceMi(double Mi) : IDistance
    {
        private static readonly double MiToKmRatio = MiToMRatio / KmToMRatio;

        public double M => Mi * MiToMRatio;
        public double Km => Mi * MiToKmRatio;

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

    static ISpeed FromMPS(double mps) => new Speed(IDistance.FromM(mps), TimeSpan.FromSeconds(1));
    static ISpeed FromKmPH(double kmPh) => new Speed(IDistance.FromKm(kmPh), TimeSpan.FromHours(1));
    static ISpeed FromMiPH(double miPh) => new Speed(IDistance.FromMi(miPh), TimeSpan.FromHours(1));

    private class Speed : ISpeed
    {
        public Speed(IDistance distance, TimeSpan time)
        {
            Distance = distance;
            Time = time;
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
    private static readonly double GToMPS2 = 9.80665d;
    private static readonly double ApproxGToMPS2 = 9.81d;

    double MPS2 { get; }
    double G { get; }
    double ApproxG { get; }

    static IAcceleration FromMPS2(double value) => new AccelerationMPS2(value);
    static IAcceleration FromG(double value) => new AccelerationG(value);
    static IAcceleration FromApproxG(double value) => new AccelerationApproxG(value);

    private record AccelerationMPS2(double MPS2) : IAcceleration
    {
        private static readonly double MPS2ToG = 1d / GToMPS2;
        private static readonly double MPS2ToApproxG = 1d / ApproxGToMPS2;

        public double G => MPS2 * MPS2ToG;
        public double ApproxG => MPS2 * MPS2ToApproxG;
    }

    private record AccelerationG(double G) : IAcceleration
    {
        private static readonly double GToApproxG = ApproxGToMPS2 / GToMPS2;

        public double MPS2 => G * GToMPS2;
        public double ApproxG => G * GToApproxG;
    }

    private record AccelerationApproxG(double ApproxG) : IAcceleration
    {
        private static readonly double ApproxGToG = GToMPS2 / ApproxGToMPS2;

        public double MPS2 => G * GToMPS2;
        public double G => ApproxG * ApproxGToG;
    }
}

public interface IAngle
{
    private static readonly double RevToRadRatio = 2d * Math.PI;
    private static readonly double RevToDegRatio = 360d;

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

    static IAngle FromDeg(double value) => new AngleDegrees(value);
    static IAngle FromRad(double value) => new AngleRadians(value);
    static IAngle FromRev(double value) => new AngleRevolutions(value);

    private record AngleRadians(double Rad) : IAngle
    {
        private static readonly double DegRatio = RevToDegRatio / RevToRadRatio;
        private static readonly double RevRatio = 1d / RevToRadRatio;

        public double Deg => Rad * DegRatio;
        public double Rev => Rad * RevRatio;
    }

    private record AngleDegrees(double Deg) : IAngle
    {
        private static readonly double RadRatio = RevToRadRatio / RevToDegRatio;
        private static readonly double RevRatio = 1d / RevToDegRatio;

        public double Rad => Deg * RadRatio;
        public double Rev => Deg * RevRatio;
    }

    private record AngleRevolutions(double Rev) : IAngle
    {
        public double Deg => Rev * RevToDegRatio;
        public double Rad => Rev * RevToRadRatio;
    }
}

public interface IAngularSpeed
{
    double RadPS { get; }
    double RevPS { get; }

    static IAngularSpeed FromRadPS(double value) => new AngularSpeed(IAngle.FromRad(value));
    static IAngularSpeed FromRevPS(double value) => new AngularSpeed(IAngle.FromRev(value));

    private class AngularSpeed : IAngularSpeed
    {
        public AngularSpeed(IAngle angle)
        {
            _angle = angle;
        }

        private readonly IAngle _angle;

        public double RadPS => _angle.Rad;
        public double RevPS => _angle.Rev;
    }
}

public record Orientation(
    IAngle Yaw,
    IAngle Pitch,
    IAngle Roll
);

public interface ITemperature
{
    private static readonly double CToKDiff = 273.15d;
    private static readonly double CToFDiff = 32d;
    private static readonly double KCToFRatio = 9d / 5d;

    double K { get; }
    double C { get; }
    double F { get; }

    static ITemperature FromK(double value) => new TemperatureKelvin(value);
    static ITemperature FromC(double value) => new TemperatureCelsius(value);
    static ITemperature FromF(double value) => new TemperatureFahrenheit(value);

    public record TemperatureKelvin(double K) : ITemperature
    {
        public double C => K - CToKDiff;
        public double F => C * KCToFRatio + CToFDiff;
    }

    public record TemperatureCelsius(double C) : ITemperature
    {
        public double K => C + CToKDiff;
        public double F => C * KCToFRatio + CToFDiff;
    }

    public record TemperatureFahrenheit(double F) : ITemperature
    {
        private static readonly double FToKCRatio = 1 / KCToFRatio;

        public double K => C + CToKDiff;
        public double C => (F - CToFDiff) * FToKCRatio;
    }
}

public interface ICapacity
{
    double L { get; }

    static ICapacity FromL(double value) => new CapacityLiters(value);

    public record CapacityLiters(double L) : ICapacity;
}

public interface IPressure
{
    double Psi { get; }
    double Kpa { get; }
    double Bar { get; }

    static IPressure FromPsi(double value) => new PressurePsi(value);
    static IPressure FromKpa(double value) => new PressureKpa(value);
    static IPressure FromBar(double value) => new PressureBar(value);

    public record PressurePsi(double Psi) : IPressure
    {
        public static readonly double PsiToKpaRatio = 6.894757;
        public static readonly double PsiToBarRatio = PsiToKpaRatio * PressureBar.BarToKpaRatio;

        public double Kpa => Psi * PsiToKpaRatio;
        public double Bar => Psi * PsiToKpaRatio;
    }

    public record PressureKpa(double Kpa) : IPressure
    {
        public static readonly double KpaToPsiRatio = 1 / PressurePsi.PsiToKpaRatio;
        public static readonly double KapToBarRatio = 1 / PressureBar.BarToKpaRatio;

        public double Psi => Kpa * KpaToPsiRatio;
        public double Bar => Kpa * KapToBarRatio;
    }

    public record PressureBar(double Bar) : IPressure
    {
        public static readonly double BarToPsiRatio = 1 / PressurePsi.PsiToBarRatio;
        public static readonly double BarToKpaRatio = 100;

        public double Psi => Bar * BarToPsiRatio;
        public double Kpa => Bar * BarToKpaRatio;
    }
}