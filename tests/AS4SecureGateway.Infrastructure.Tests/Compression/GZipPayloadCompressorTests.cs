using System.IO.Compression;
using System.Text;
using AS4SecureGateway.Infrastructure.Compression;
using FluentAssertions;

namespace AS4SecureGateway.Infrastructure.Tests.Compression;

public sealed class GZipPayloadCompressorTests
{
    private readonly GZipPayloadCompressor _compressor = new();

    [Fact]
    public void Compress_ShouldReturnGZipPayloadThatCanBeDecompressedToOriginalContent()
    {
        var originalText = "<Document><Value>AS4 secure gateway</Value></Document>";
        var payload = Encoding.UTF8.GetBytes(originalText);

        var compressed = _compressor.Compress(payload);
        var decompressed = DecompressToString(compressed);

        compressed.Should().NotBeEmpty();
        compressed.Should().NotBeEquivalentTo(payload);
        decompressed.Should().Be(originalText);
    }

    [Fact]
    public void Compress_WhenPayloadIsNull_ShouldThrowArgumentNullException()
    {
        var act = () => _compressor.Compress(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Compress_WhenPayloadIsEmpty_ShouldThrowArgumentException()
    {
        var act = () => _compressor.Compress(Array.Empty<byte>());

        act.Should().Throw<ArgumentException>()
            .Where(exception => exception.ParamName == "payload");
    }

    private static string DecompressToString(byte[] compressedPayload)
    {
        using var inputStream = new MemoryStream(compressedPayload);
        using var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress);
        using var reader = new StreamReader(gzipStream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
}