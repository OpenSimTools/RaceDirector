namespace RaceDirector.Remote.Networking.Codec;

public interface IEncoder<in T>
{
    ReadOnlySpan<byte> Encode(T t);
}

public class Encoder<T> : IEncoder<T>
{
    public delegate ReadOnlySpan<byte> EncodeF(T t);

    private readonly EncodeF _f;
    public static IEncoder<T> From(EncodeF f) => new Encoder<T>(f);

    private Encoder(EncodeF f)
    {
        _f = f;
    }

    public ReadOnlySpan<byte> Encode(T t) => _f(t);
}

public static class EncoderEx
{
    public static ICodec<T, Nothing> ToCodec<T>(this IEncoder<T> encoder)
        => new EncoderOnly<T>(encoder);

    private record EncoderOnly<T>(IEncoder<T> Encoder) : ICodec<T, Nothing>
    {
        public ReadOnlySpan<byte> Encode(T t) => Encoder.Encode(t);

        public Nothing Decode(ReadOnlySpan<byte> ba) => new();
    }
}