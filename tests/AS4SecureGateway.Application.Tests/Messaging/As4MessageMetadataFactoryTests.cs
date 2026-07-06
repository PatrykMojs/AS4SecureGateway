using AS4SecureGateway.Application.Messaging;
using FluentAssertions;

namespace AS4SecureGateway.Application.Tests.Messaging;

public sealed class As4MessageMetadataFactoryTests
{
    private readonly As4MessageMetadataFactory _factory = new();

    [Fact]
    public void Create_ForSendMessageWithoutCompression_ShouldCreateBaseMetadata()
    {
        var options = new SecurityProcessingOptions();

        var metadata = _factory.Create(
            As4ActionType.SendMessage,
            options,
            senderPartyId: "sender-party",
            senderRole: "sender-role",
            receiverPartyId: "receiver-party");

        metadata.SenderPartyId.Should().Be("sender-party");
        metadata.SenderRole.Should().Be("sender-role");
        metadata.ReceiverPartyId.Should().Be("receiver-party");
        metadata.ReceiverRole.Should().Be("Receiver");
        metadata.Service.Should().Be("DemoMarketMessaging");
        metadata.Action.Should().Be("SendMessage");
        metadata.AgreementRef.Should().Be("urn:demo:as4:agreement:SendMessage");
        metadata.MimeType.Should().BeNull();
        metadata.CharacterSet.Should().BeNull();
        metadata.CompressionType.Should().BeNull();
        metadata.ConversationId.Should().StartWith($"{DateTime.UtcNow:yyyy}-");

        var guidPart = metadata.ConversationId.Split('-', 2)[1];
        Guid.TryParseExact(guidPart, "N", out _).Should().BeTrue();
    }

    [Fact]
    public void Create_ForPeekMessageWithFullSecurity_ShouldCreateMetadataWithSecuritySuffixAndCompressionProperties()
    {
        var options = new SecurityProcessingOptions
        {
            EnableCompression = true,
            EnableEncryption = true,
            EnableSignature = true
        };

        var metadata = _factory.Create(
            As4ActionType.PeekMessage,
            options,
            senderPartyId: "sender-party",
            senderRole: "sender-role",
            receiverPartyId: "receiver-party");

        metadata.Action.Should().Be("PeekMessage.Request");
        metadata.AgreementRef.Should().Be("urn:demo:as4:agreement:PeekMessage.Request:Compression:Encryption:Signature");
        metadata.MimeType.Should().Be("application/xml");
        metadata.CharacterSet.Should().Be("utf-8");
        metadata.CompressionType.Should().Be("application/gzip");
    }

    [Fact]
    public void Create_WhenSecurityOptionsIsNull_ShouldThrowArgumentNullException()
    {
        var act = () => _factory.Create(
            As4ActionType.SendMessage,
            securityOptions: null!,
            senderPartyId: "sender-party",
            senderRole: "sender-role",
            receiverPartyId: "receiver-party");

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("", "sender-role", "receiver-party", "senderPartyId")]
    [InlineData("sender-party", "", "receiver-party", "senderRole")]
    [InlineData("sender-party", "sender-role", "", "receiverPartyId")]
    public void Create_WhenRequiredPartyFieldIsEmpty_ShouldThrowArgumentException(
        string senderPartyId,
        string senderRole,
        string receiverPartyId,
        string expectedParamName)
    {
        var act = () => _factory.Create(
            As4ActionType.SendMessage,
            new SecurityProcessingOptions(),
            senderPartyId,
            senderRole,
            receiverPartyId);

        act.Should().Throw<ArgumentException>()
            .Where(exception => exception.ParamName == expectedParamName);
    }

    [Fact]
    public void Create_WhenActionIsUnsupported_ShouldThrowInvalidOperationException()
    {
        var act = () => _factory.Create(
            (As4ActionType)999,
            new SecurityProcessingOptions(),
            senderPartyId: "sender-party",
            senderRole: "sender-role",
            receiverPartyId: "receiver-party");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Unsupported action type: 999*");
    }
}