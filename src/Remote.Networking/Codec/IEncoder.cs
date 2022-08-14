﻿namespace RaceDirector.Remote.Networking.Codec;

public interface IEncoder<in T>
{
    ReadOnlyMemory<byte> Encode(T t);
}

public class Encoder<T> : IEncoder<T>
{
    public delegate ReadOnlyMemory<byte> EncodeF(T t);

    private readonly EncodeF _f;
    public static IEncoder<T> From(EncodeF f) => new Encoder<T>(f);

    private Encoder(EncodeF f)
    {
        _f = f;
    }

    public ReadOnlyMemory<byte> Encode(T t) => _f(t);
}

public static class EncoderEx
{
    public static IEncoder<TIn> Wrap<TIn, TOut>(this IEncoder<TOut> encoder, Func<TIn, TOut> f)
        => Encoder<TIn>.From(t => encoder.Encode(f(t)));

    public static ICodec<T, Nothing> ToCodec<T>(this IEncoder<T> encoder)
        => new EncoderOnly<T>(encoder);

    private record EncoderOnly<T>(IEncoder<T> Encoder) : ICodec<T, Nothing>
    {
        public ReadOnlyMemory<byte> Encode(T t) => Encoder.Encode(t);

        public Nothing Decode(ReadOnlyMemory<byte> payload) => new();
    }
}