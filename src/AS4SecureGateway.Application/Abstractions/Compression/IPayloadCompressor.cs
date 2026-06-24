namespace AS4SecureGateway.Application.Abstraction.Compression;

public interface IPayloadCompressor
{
    byte[] Compress(byte[] payload);
}