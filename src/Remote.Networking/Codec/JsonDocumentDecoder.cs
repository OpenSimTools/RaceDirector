using System.Text.Json;

namespace RaceDirector.Remote.Networking.Codec;

public class JsonDocumentDecoder : IDecoder<JsonDocument>
{
    // Might try and see if we can use ReadOnlyMemory (that has a Span property) instead of ReadOnlySpan.
    // https://docs.microsoft.com/en-us/archive/msdn-magazine/2018/january/csharp-all-about-span-exploring-a-new-net-mainstay
    public JsonDocument Decode(ReadOnlySpan<byte> payload) => JsonDocument.Parse(payload.ToArray());
}