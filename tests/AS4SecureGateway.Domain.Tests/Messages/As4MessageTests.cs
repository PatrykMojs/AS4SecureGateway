using System.Text;
using AS4SecureGateway.Domain.Messages;
using FluentAssertions;

namespace AS4SecureGateway.Domain.Tests.Messages;

public sealed class As4MessageTests
{
    [Fact]
    public void Create_WithValidValues_ShouldCreatePendingMessage()
    {
        var sender = new Party("sender-party", "sender-role");
        var receiver = new Party("receiver-party", "receiver-role");
        var collaborationInfo = new CollaborationInfo("agreement", "service", "action");
        var payload = new As4Payload("payload-id", "application/xml", "utf-8", Encoding.UTF8.GetBytes("<Payload />"));

        var message = As4Message.Create("message-id", "conversation-id", sender, receiver, collaborationInfo, payload);

        message.Id.Should().NotBeEmpty();
        message.MessageId.Should().Be("message-id");
        message.ConversationId.Should().Be("conversation-id");
        message.Sender.Should().BeSameAs(sender);
        message.Receiver.Should().BeSameAs(receiver);
        message.CollaborationInfo.Should().BeSameAs(collaborationInfo);
        message.Payload.Should().BeSameAs(payload);
        message.Status.Should().Be(As4MessageStatus.Pending);
        message.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        message.SentAtUtc.Should().BeNull();
        message.LastError.Should().BeNull();
    }

    [Theory]
    [InlineData("", "conversation-id", "messageId")]
    [InlineData("message-id", "", "conversationId")]
    public void Create_WhenRequiredIdIsEmpty_ShouldThrowArgumentException(string messageId, string conversationId, string expectedParamName)
    {
        var act = () => As4Message.Create(
            messageId,
            conversationId,
            new Party("sender-party", "sender-role"),
            new Party("receiver-party", "receiver-role"),
            new CollaborationInfo("agreement", "service", "action"),
            new As4Payload("payload-id", "application/xml", "utf-8", Encoding.UTF8.GetBytes("<Payload />")));

        act.Should().Throw<ArgumentException>()
            .Where(exception => exception.ParamName == expectedParamName);
    }

    [Fact]
    public void MarkAsProcessing_ShouldSetProcessingStatusAndClearLastError()
    {
        var message = CreateMessage();
        message.MarkAsFailed("Temporary error");

        message.MarkAsProcessing();

        message.Status.Should().Be(As4MessageStatus.Processing);
        message.LastError.Should().BeNull();
    }

    [Fact]
    public void MarkAsSent_ShouldSetSentStatusSentAtAndClearLastError()
    {
        var message = CreateMessage();
        message.MarkAsFailed("Temporary error");

        message.MarkAsSent();

        message.Status.Should().Be(As4MessageStatus.Sent);
        message.SentAtUtc.Should().NotBeNull();
        message.SentAtUtc!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        message.LastError.Should().BeNull();
    }

    [Fact]
    public void MarkAsFailed_ShouldSetFailedStatusAndStoreError()
    {
        var message = CreateMessage();

        message.MarkAsFailed("HTTP 500");

        message.Status.Should().Be(As4MessageStatus.Failed);
        message.LastError.Should().Be("HTTP 500");
    }

    private static As4Message CreateMessage() => As4Message.Create(
        "message-id",
        "conversation-id",
        new Party("sender-party", "sender-role"),
        new Party("receiver-party", "receiver-role"),
        new CollaborationInfo("agreement", "service", "action"),
        new As4Payload("payload-id", "application/xml", "utf-8", Encoding.UTF8.GetBytes("<Payload />")));
}