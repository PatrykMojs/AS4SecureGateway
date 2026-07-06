using System.IO.Compression;
using AS4SecureGateway.Application.Abstractions.Compression;

namespace AS4SecureGateway.Infrastructure.Compression;

public sealed class GZipPayloadDecompressor : IPayloadDecompressor
{
    public byte[] Decompress(byte[] compressedPayload)
    {
        ArgumentNullException.ThrowIfNull(compressedPayload);

        using var input = new MemoryStream(compressedPayload);
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();

        gzip.CopyTo(output);

        return output.ToArray();
    }
}