namespace RaceDirector.Remote.Networking.Codec;

public class IdentityCodec : ICodec<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>
{
    public ReadOnlyMemory<byte> Encode(ReadOnlyMemory<byte> payload) => payload;

    public ReadOnlyMemory<byte> Decode(ReadOnlyMemory<byte> payload) => payload;
}