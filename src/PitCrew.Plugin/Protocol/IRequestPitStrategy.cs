namespace RaceDirector.PitCrew.Protocol;

public interface IRequestPitStrategy
{
    double? FuelToAdd { get; }
}

public class RequestPitStrategy : IRequestPitStrategy
{
    public double? FuelToAdd { get; set; } = null;
}