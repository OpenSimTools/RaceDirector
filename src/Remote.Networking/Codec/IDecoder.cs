namespace RaceDirector.Remote.Networking.Codec;

public interface IDecoder<out T>
{
    T Decode(ReadOnlySpan<byte> payload);
}

public static class DecoderEx
{
    public static ICodec<Nothing, T> ToCodec<T>(this IDecoder<T> decoder)
        => new DecoderOnly<T>(decoder);

    private record DecoderOnly<T>(IDecoder<T> Decoder) : ICodec<Nothing, T>
    {
        public ReadOnlySpan<byte> Encode(Nothing t) => Array.Empty<byte>();

        public T Decode(ReadOnlySpan<byte> payload) => Decoder.Decode(payload);
    }
}