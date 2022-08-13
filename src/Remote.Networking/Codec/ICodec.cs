namespace RaceDirector.Remote.Networking.Codec;

public interface ICodec<in TE, out TD> : IEncoder<TE>, IDecoder<TD> { }
