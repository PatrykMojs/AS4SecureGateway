using System.IO.Compression;

namespace AS4SecureGateway.TestEndpoint.Security;

public sealed class TestEndpointGZipDecompressor
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