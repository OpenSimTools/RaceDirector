using System.Text.Json;

namespace RaceDirector.Remote.Networking.Codec;

public class JsonEncoder<T> : IEncoder<T>
{
    /// <summary>
    /// Serialises not-null message to JSON.
    /// </summary>
    /// <param name="t">Message</param>
    /// <returns>JSON payload</returns>
    public ReadOnlyMemory<byte> Encode(T? t) => JsonSerializer.SerializeToUtf8Bytes(t);
}