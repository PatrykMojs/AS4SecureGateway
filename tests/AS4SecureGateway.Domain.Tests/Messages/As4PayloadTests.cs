using System.Text;
using AS4SecureGateway.Domain.Messages;
using FluentAssertions;

namespace AS4SecureGateway.Domain.Tests.Messages;

public sealed class As4PayloadTests
{
    [Fact]
    public void Constructor_WithValidValues_ShouldCreatePayload()
    {
        var content = Encoding.UTF8.GetBytes("<Document>Test</Document>");

        var payload = new As4Payload(
            "payload@as4-secure-gateway",
            "application/xml",
            "utf-8",
            content,
            "application/gzip");

        payload.ContentId.Should().Be("payload@as4-secure-gateway");
        payload.MimeType.Should().Be("application/xml");
        payload.CharacterSet.Should().Be("utf-8");
        payload.CompressionType.Should().Be("application/gzip");
        payload.Content.Should().BeEquivalentTo(content);
    }

    [Theory]
    [InlineData("", "application/xml", "utf-8", "contentId")]
    [InlineData("payload@as4-secure-gateway", "", "utf-8", "mimeType")]
    [InlineData("payload@as4-secure-gateway", "application/xml", "", "characterSet")]
    public void Constructor_WhenRequiredTextValueIsEmpty_ShouldThrowArgumentException(
        string contentId,
        string mimeType,
        string characterSet,
        string expectedParamName)
    {
        var content = Encoding.UTF8.GetBytes("<Document>Test</Document>");

        var act = () => new As4Payload(contentId, mimeType, characterSet, content);

        act.Should().Throw<ArgumentException>()
            .Where(exception => exception.ParamName == expectedParamName);
    }

    [Fact]
    public void Constructor_WhenContentIsEmpty_ShouldThrowArgumentException()
    {
        var act = () => new As4Payload("payload@as4-secure-gateway", "application/xml", "utf-8", Array.Empty<byte>());

        act.Should().Throw<ArgumentException>()
            .Where(exception => exception.ParamName == "content");
    }
}
