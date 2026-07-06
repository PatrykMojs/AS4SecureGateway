using AS4SecureGateway.Application.Messaging;
using AS4SecureGateway.Application.UseCases.DispatchAs4Message;
using AS4SecureGateway.Infrastructure.Messaging;
using FluentAssertions;

namespace AS4SecureGateway.Infrastructure.Tests.Messaging;

public sealed class As4BusinessBodyFactoryTests
{
    private readonly As4BusinessBodyFactory _factory = new();

    [Fact]
    public void CreateBody_ForSendMessage_ShouldCreateSendMessageRequestWithPayloadAndScenario()
    {
        var command = new DispatchAs4MessageCommand
        {
            ActionType = As4ActionType.SendMessage,
            PayloadXml = "<Invoice><Id>FV/1/2026</Id></Invoice>",
            TestScenario = As4TestScenario.Accepted
        };

        var body = _factory.CreateBody(command);

        body.Should().Contain("SendMessageRequest");
        body.Should().Contain("MessageContainer");
        body.Should().Contain(command.PayloadXml);
        body.Should().Contain("Accepted");
        body.Should().Contain("urn:demo:as4:business-message:v1");
    }

    [Fact]
    public void CreateBody_ForSendMessageWithoutPayload_ShouldThrowInvalidOperationException()
    {
        var command = new DispatchAs4MessageCommand
        {
            ActionType = As4ActionType.SendMessage,
            PayloadXml = ""
        };

        var act = () => _factory.CreateBody(command);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Payload XML is required for SendMessage.");
    }

    [Fact]
    public void CreateBody_ForPeekMessageWithoutDomain_ShouldCreateEmptyPeekRequest()
    {
        var command = new DispatchAs4MessageCommand
        {
            ActionType = As4ActionType.PeekMessage,
            PeekMessageDomain = null
        };

        var body = _factory.CreateBody(command);

        body.Should().Contain("PeekMessageRequest");
        body.Should().NotContain("MessageDomain");
        body.Should().Contain("urn:demo:as4:business-message:v1");
    }

    [Fact]
    public void CreateBody_ForPeekMessageWithDomain_ShouldEscapeDomainValue()
    {
        var command = new DispatchAs4MessageCommand
        {
            ActionType = As4ActionType.PeekMessage,
            PeekMessageDomain = "energy & <grid>"
        };

        var body = _factory.CreateBody(command);

        body.Should().Contain("PeekMessageRequest");
        body.Should().Contain("MessageDomain");
        body.Should().Contain("energy &amp; &lt;grid&gt;");
    }

    [Fact]
    public void CreateBody_WhenCommandIsNull_ShouldThrowArgumentNullException()
    {
        var act = () => _factory.CreateBody(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CreateBody_WhenActionIsUnsupported_ShouldThrowInvalidOperationException()
    {
        var command = new DispatchAs4MessageCommand
        {
            ActionType = (As4ActionType)999
        };

        var act = () => _factory.CreateBody(command);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Unsupported AS4 action*");
    }
}
