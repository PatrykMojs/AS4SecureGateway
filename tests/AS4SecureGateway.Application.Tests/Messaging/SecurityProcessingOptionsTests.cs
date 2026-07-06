using AS4SecureGateway.Application.Messaging;
using FluentAssertions;

namespace AS4SecureGateway.Application.Tests.Messaging;

public sealed class SecurityProcessingOptionsTests
{
    [Theory]
    [InlineData(false, false, false, "")]
    [InlineData(true, false, false, ":Compression")]
    [InlineData(false, true, false, ":Encryption")]
    [InlineData(false, false, true, ":Signature")]
    [InlineData(true, true, false, ":Compression:Encryption")]
    [InlineData(true, false, true, ":Compression:Signature")]
    [InlineData(false, true, true, ":Encryption:Signature")]
    [InlineData(true, true, true, ":Compression:Encryption:Signature")]
    public void GetAgreementSuffix_ShouldReturnExpectedSuffix(
        bool compression,
        bool encryption,
        bool signature,
        string expectedSuffix)
    {
        var options = new SecurityProcessingOptions
        {
            EnableCompression = compression,
            EnableEncryption = encryption,
            EnableSignature = signature
        };

        var result = options.GetAgreementSuffix();

        result.Should().Be(expectedSuffix);
    }

    [Theory]
    [InlineData(false, false, false, "")]
    [InlineData(true, false, false, "Compression")]
    [InlineData(false, true, false, "Encryption")]
    [InlineData(false, false, true, "Signature")]
    [InlineData(false, true, true, "EncryptionSignature")]
    [InlineData(true, true, true, "CompressionEncryptionSignature")]
    public void GetEndpointPathSegment_ShouldReturnExpectedSupportedSegment(
        bool compression,
        bool encryption,
        bool signature,
        string expectedSegment)
    {
        var options = new SecurityProcessingOptions
        {
            EnableCompression = compression,
            EnableEncryption = encryption,
            EnableSignature = signature
        };

        var result = options.GetEndpointPathSegment();

        result.Should().Be(expectedSegment);
    }
}
