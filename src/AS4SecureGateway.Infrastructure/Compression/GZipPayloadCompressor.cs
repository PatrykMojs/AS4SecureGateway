using System.IO.Compression;
using AS4SecureGateway.Application.Abstraction.Compression;

namespace AS4SecureGateway.Infrastructure.Compression;

public sealed class GZipPayloadCompressor : IPayloadCompressor
{
    public byte[] Compress(byte[] payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        if(payload.Length == 0)
            throw new ArgumentException("Payload cannot be empty.", nameof(payload));

        using var memoryStream = new MemoryStream();

        using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true))
        {
            gzipStream.Write(payload, 0, payload.Length);
        }

        return memoryStream.ToArray();
    }
}