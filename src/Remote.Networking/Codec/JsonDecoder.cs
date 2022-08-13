using System.Text.Json;

namespace RaceDirector.Remote.Networking.Codec;

public class JsonDecoder<T> : IDecoder<T>
{
    /// <summary>
    /// Deserialises not-null JSON payload.
    /// </summary>
    /// <param name="payload">JSON UTF8 bytes</param>
    /// <returns>Converted message</returns>
    public T Decode(ReadOnlySpan<byte> payload) => JsonSerializer.Deserialize<T>(payload)!;
}