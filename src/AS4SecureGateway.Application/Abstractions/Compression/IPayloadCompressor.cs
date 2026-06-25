namespace AS4SecureGateway.Application.Abstractions.Compression;

public interface IPayloadCompressor
{
    byte[] Compress(byte[] payload);
}