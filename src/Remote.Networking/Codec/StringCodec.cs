using System.Text;

namespace RaceDirector.Remote.Networking.Codec;

public class StringCodec : ICodec<string, string>
{
    public static StringCodec UTF8 => new(Encoding.UTF8);

    private readonly Encoding _encoding;

    public StringCodec(Encoding encoding)
    {
        _encoding = encoding;
    }

    public ReadOnlyMemory<byte> Encode(string? t) => t is null ? Array.Empty<byte>() : _encoding.GetBytes(t);

    public string Decode(ReadOnlyMemory<byte> payload) => _encoding.GetString(payload.Span);
}