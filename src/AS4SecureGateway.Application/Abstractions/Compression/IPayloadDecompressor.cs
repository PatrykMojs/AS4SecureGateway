namespace AS4SecureGateway.Application.Abstractions.Compression;

public interface IPayloadDecompressor
{
    byte[] Decompress(byte[] compressedPayload);
}