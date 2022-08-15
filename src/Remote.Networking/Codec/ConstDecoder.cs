namespace RaceDirector.Remote.Networking.Codec;

public record ConstDecoder<T>(T Value) : IDecoder<T>
{
    public T Decode(ReadOnlyMemory<byte> payload) => Value;
}

public static class ConstDecoder
{
    public static IDecoder<Nothing> Nothing => new ConstDecoder<Nothing>(new Nothing());
}