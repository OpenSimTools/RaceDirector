using System.Text.Json;

namespace RaceDirector.Remote.Networking.Codec;

public class JsonDocumentDecoder : IDecoder<JsonDocument>
{
    public JsonDocument Decode(ReadOnlyMemory<byte> payload) => JsonDocument.Parse(payload);
}