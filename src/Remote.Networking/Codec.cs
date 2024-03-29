﻿using System.Text;
using System.Text.Json;

namespace RaceDirector.Remote.Networking;

public readonly struct Codec<TE, TD>
{
    public Encode<TE> Encode { get; init; }
    public Decode<TD> Decode { get; init; }
}

public delegate ReadOnlyMemory<byte> Encode<in T>(T t);

public static class EncodeEx
{
    public static Encode<TA> Select<TA, TB>(this Encode<TB> encode, Func<TA, TB> f) =>
        a => encode(f(a));

    public static Encode<T?> IgnoreNull<T>(this Encode<T> encode) =>
        t => t is null ? ReadOnlyMemory<byte>.Empty : encode(t);
}

public delegate T Decode<out T>(ReadOnlyMemory<byte> payload);

public static class DecodeEx
{
    public static Decode<TB> Select<TA, TB>(this Decode<TA> decode, Func<TA, TB> f) =>
        payload => f(decode(payload));

    public static Decode<T?> IgnoreErrors<T>(this Decode<T> decode) =>
        payload =>
        {
            try
            {
                return decode(payload);
            }
            catch
            {
                return default;
            }
        };
}

public static class Codec
{
    public static Codec<string, string> UTF8String = String(Encoding.UTF8);

    public static Codec<string, string> String(Encoding encoding) => new()
    {

        Encode = t => encoding.GetBytes(t),
        Decode = payload => encoding.GetString(payload.Span)
    };
    
    public static Codec<TE, TD> Json<TE, TD>() => new()
    {
        Encode = JsonEncode<TE>(),
        Decode = JsonDecode<TD>()
    };
    
    public static Encode<T> JsonEncode<T>(JsonSerializerOptions? options = null) => t => JsonSerializer.SerializeToUtf8Bytes(t, options);
    public static Decode<T> JsonDecode<T>(JsonSerializerOptions? options = null) => payload => JsonSerializer.Deserialize<T>(payload.Span, options)!;

    public static Codec<Nothing, JsonDocument> JsonDocument = new()
    {
        Encode = _ => ReadOnlyMemory<byte>.Empty, // TODO
        Decode = payload => System.Text.Json.JsonDocument.Parse(payload)
    };

    public static Codec<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>> Identity = new()
    {
        Encode = i => i,
        Decode = i => i
    };
    
    public static Codec<Nothing, Nothing> Nothing => new()
    {

        Encode = _ => Array.Empty<byte>(),
        Decode = _ => new Nothing()
    };
    
    public static Codec<Nothing, T> DecodeOnly<T>(Decode<T> decode) => new()
    {

        Encode = Nothing.Encode,
        Decode = decode
    };
    
    public static Codec<T, Nothing> EncodeOnly<T>(Encode<T> encode) => new()
    {

        Encode = encode,
        Decode = Nothing.Decode
    };
}