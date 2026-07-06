using System.Xml.Linq;
using AS4SecureGateway.Application.Messaging;
using AS4SecureGateway.Infrastructure.Messaging;
using FluentAssertions;

namespace AS4SecureGateway.Infrastructure.Tests.Messaging;

public sealed class As4EnvelopeFactoryTests
{
    private readonly As4EnvelopeFactory _factory = new();

    [Fact]
    public void CreateEnvelope_ShouldCreateSoapEnvelopeWithEbmsUserMessage()
    {
        var metadata = CreateMetadata();

        var document = _factory.CreateEnvelope(metadata, As4MessageDefaults.PayloadHref);
        var xml = XDocument.Parse(document.OuterXml);

        xml.Root!.Name.LocalName.Should().Be("Envelope");
        xml.Descendants().Should().Contain(element => element.Name.LocalName == "Header");
        xml.Descendants().Should().Contain(element => element.Name.LocalName == "Body");
        xml.Descendants().Should().Contain(element => element.Name.LocalName == "Messaging");
        xml.Descendants().Should().Contain(element => element.Name.LocalName == "UserMessage");

        xml.Descendants().Where(element => element.Name.LocalName == "PartyId")
            .Select(element => element.Value)
            .Should()
            .Contain(new[] { "sender-party", "receiver-party" });

        xml.Descendants().Where(element => element.Name.LocalName == "Role")
            .Select(element => element.Value)
            .Should()
            .Contain("sender-role");

        xml.Descendants().Single(element => element.Name.LocalName == "AgreementRef")
            .Value.Should().Be(metadata.AgreementRef);

        xml.Descendants().Single(element => element.Name.LocalName == "Service")
            .Value.Should().Be(metadata.Service);

        xml.Descendants().Single(element => element.Name.LocalName == "Action")
            .Value.Should().Be(metadata.Action);

        xml.Descendants().Single(element => element.Name.LocalName == "ConversationId")
            .Value.Should().Be(metadata.ConversationId);

        var partInfo = xml.Descendants().Single(element => element.Name.LocalName == "PartInfo");

        partInfo.Attribute("href")!.Value.Should().Be(As4MessageDefaults.PayloadHref);
    }

    [Fact]
    public void CreateEnvelope_WhenPayloadHasCompressionMetadata_ShouldCreatePartProperties()
    {
        var metadata = CreateMetadata(
            mimeType: "application/xml",
            characterSet: "utf-8",
            compressionType: "application/gzip");

        var document = _factory.CreateEnvelope(metadata, As4MessageDefaults.PayloadHref);
        var xmlText = document.OuterXml;

        xmlText.Should().Contain("MimeType");
        xmlText.Should().Contain("application/xml");
        xmlText.Should().Contain("CharacterSet");
        xmlText.Should().Contain("utf-8");
        xmlText.Should().Contain("CompressionType");
        xmlText.Should().Contain("application/gzip");
    }

    [Fact]
    public void CreateEnvelope_WhenMetadataIsNull_ShouldThrowArgumentNullException()
    {
        var act = () => _factory.CreateEnvelope(null!, As4MessageDefaults.PayloadHref);

        act.Should().Throw<ArgumentNullException>();
    }

    private static As4MessageMetadata CreateMetadata(
        string? mimeType = null,
        string? characterSet = null,
        string? compressionType = null) => new()
    {
        SenderPartyId = "sender-party",
        SenderRole = "sender-role",
        ReceiverPartyId = "receiver-party",
        ReceiverRole = "receiver-role",
        AgreementRef = "urn:demo:as4:agreement:SendMessage:Compression",
        Service = "bdx:noprocess",
        Action = "SendMessage",
        ConversationId = "2026-0123456789abcdef0123456789abcdef",
        MimeType = mimeType,
        CharacterSet = characterSet,
        CompressionType = compressionType
    };
}