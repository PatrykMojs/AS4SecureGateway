using System.Net;
using System.Xml;
using AS4SecureGateway.Application.Abstractions.Messaging;
using AS4SecureGateway.Application.Abstractions.Persistence;
using AS4SecureGateway.Application.Abstractions.Responses;
using AS4SecureGateway.Application.Messaging;
using AS4SecureGateway.Application.UseCases.DispatchAs4Message;
using FluentAssertions;
using NSubstitute;

namespace AS4SecureGateway.Application.Tests.UseCases;

public sealed class DispatchAs4MessageHandlerTests
{
    private readonly IAs4BusinessBodyFactory _businessBodyFactory = Substitute.For<IAs4BusinessBodyFactory>();
    private readonly IAs4EnvelopeFactory _envelopeFactory = Substitute.For<IAs4EnvelopeFactory>();
    private readonly IAs4SecurityPipeline _securityPipeline = Substitute.For<IAs4SecurityPipeline>();
    private readonly IAs4TransportClient _transportClient = Substitute.For<IAs4TransportClient>();
    private readonly IAs4ResponseParser _responseParser = Substitute.For<IAs4ResponseParser>();
    private readonly IAs4MessageAuditRepository _auditRepository = Substitute.For<IAs4MessageAuditRepository>();

    private DispatchAs4MessageHandler CreateHandler() => new(
        new As4MessageMetadataFactory(),
        _businessBodyFactory,
        _envelopeFactory,
        _securityPipeline,
        _transportClient,
        _responseParser,
        _auditRepository);

    [Fact]
    public async Task HandleAsync_WhenCommandIsNull_ShouldThrowArgumentNullException()
    {
        var handler = CreateHandler();

        var act = () => handler.HandleAsync(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Theory]
    [InlineData("", "sender-role", "receiver-party", "SenderPartyId is required.")]
    [InlineData("sender-party", "", "receiver-party", "SenderRole is required.")]
    [InlineData("sender-party", "sender-role", "", "ReceiverPartyId is required.")]
    public async Task HandleAsync_WhenRequiredFieldIsMissing_ShouldThrowInvalidOperationException(
        string senderPartyId,
        string senderRole,
        string receiverPartyId,
        string expectedMessage)
    {
        var handler = CreateHandler();
        var command = new DispatchAs4MessageCommand
        {
            ActionType = As4ActionType.SendMessage,
            SenderPartyId = senderPartyId,
            SenderRole = senderRole,
            ReceiverPartyId = receiverPartyId,
            PayloadXml = "<Payload />"
        };

        var act = () => handler.HandleAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage(expectedMessage);
    }

    [Fact]
    public async Task HandleAsync_WhenSendMessageHasNoPayload_ShouldThrowInvalidOperationException()
    {
        var handler = CreateHandler();
        var command = new DispatchAs4MessageCommand
        {
            ActionType = As4ActionType.SendMessage,
            SenderPartyId = "sender-party",
            SenderRole = "sender-role",
            ReceiverPartyId = "receiver-party",
            PayloadXml = ""
        };

        var act = () => handler.HandleAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("PayloadXml is required for SendMessage.");
    }

    [Fact]
    public async Task HandleAsync_ForValidSendMessage_ShouldPrepareSendParseAndSaveAuditRecord()
    {
        var handler = CreateHandler();
        var command = new DispatchAs4MessageCommand
        {
            ActionType = As4ActionType.SendMessage,
            SenderPartyId = "sender-party",
            SenderRole = "sender-role",
            ReceiverPartyId = "receiver-party",
            PayloadXml = "<Payload><Value>123</Value></Payload>",
            SecurityOptions = new SecurityProcessingOptions
            {
                EnableCompression = true
            }
        };

        var envelope = CreateXmlDocument("<Envelope><Header /></Envelope>");
        var preparedEnvelope = CreateXmlDocument("<PreparedEnvelope><Header /></PreparedEnvelope>");
        var preparedMessage = new PreparedAs4Message
        {
            Envelope = preparedEnvelope,
            ContentType = "multipart/related"
        };

        var transportResult = new As4TransportResult
        {
            StatusCode = HttpStatusCode.OK,
            ResponseBody = "<Response><Status>Accepted</Status></Response>"
        };

        var parsedResponse = new As4ParsedResponse
        {
            Status = "Accepted",
            MessageId = "server-message-id",
            DocumentId = "document-id"
        };

        _businessBodyFactory.CreateBody(command)
            .Returns("<BusinessBody />");

        _envelopeFactory.CreateEnvelope(Arg.Any<As4MessageMetadata>(), As4MessageDefaults.PayloadHref)
            .Returns(envelope);

        _securityPipeline.PrepareAsync(envelope, "<BusinessBody />", command.SecurityOptions, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(preparedMessage));

        _transportClient.SendAsync(preparedMessage, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(transportResult));

        _responseParser.Parse(transportResult.ResponseBody)
            .Returns(parsedResponse);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.ActionType.Should().Be(As4ActionType.SendMessage);
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.ResponseBody.Should().Be(transportResult.ResponseBody);
        result.RequestXml.Should().Be(preparedEnvelope.OuterXml);
        result.ParsedResponse.Should().BeSameAs(parsedResponse);

        _envelopeFactory.Received(1).CreateEnvelope(
            Arg.Is<As4MessageMetadata>(metadata =>
                metadata.SenderPartyId == "sender-party" &&
                metadata.SenderRole == "sender-role" &&
                metadata.ReceiverPartyId == "receiver-party" &&
                metadata.Action == "SendMessage" &&
                metadata.CompressionType == "application/gzip"),
            As4MessageDefaults.PayloadHref);

        await _auditRepository.Received(1).SaveAsync(
            Arg.Is<As4MessageAuditRecord>(record =>
                record.ActionType == As4ActionType.SendMessage &&
                record.HttpStatusCode == 200 &&
                record.IsSuccess &&
                record.RequestXml == preparedEnvelope.OuterXml &&
                record.ResponseBody == transportResult.ResponseBody &&
                record.Status == "Accepted" &&
                record.MessageId == "server-message-id" &&
                record.DocumentId == "document-id"),
            Arg.Any<CancellationToken>());
    }

    private static XmlDocument CreateXmlDocument(string xml)
    {
        var document = new XmlDocument();
        document.LoadXml(xml);
        return document;
    }
}