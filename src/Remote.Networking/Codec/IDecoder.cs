namespace RaceDirector.Remote.Networking.Codec;

public interface IDecoder<out T>
{
    T Decode(ReadOnlyMemory<byte> payload);
}

public static class DecoderEx
{
    public static ICodec<Nothing, T> ToCodec<T>(this IDecoder<T> decoder)
        => new DecoderOnly<T>(decoder);

    private record DecoderOnly<T>(IDecoder<T> Decoder) : ICodec<Nothing, T>
    {
        public ReadOnlyMemory<byte> Encode(Nothing t) => Array.Empty<byte>();

        public T Decode(ReadOnlyMemory<byte> payload) => Decoder.Decode(payload);
    }
}