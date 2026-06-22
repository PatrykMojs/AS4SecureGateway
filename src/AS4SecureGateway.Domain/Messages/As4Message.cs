namespace AS4SecureGateway.Domain.Messages;

public sealed class As4Message
{
    public Guid Id { get; private set; }
    public string MessageId { get; private set; }
    public string ConversationId { get; private set; }
    public Party Sender { get; private set; }
    public Party Receiver { get; private set; }
    public CollaborationInfo CollaborationInfo { get; private set; }
    public As4Payload Payload { get; private set; }
    public As4MessageStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? SentAtUtc { get; private set; }
    public string? LastError { get; private set; }

    private As4Message(
        Guid id,
        string messageId,
        string conversationId,
        Party sender,
        Party receiver,
        CollaborationInfo collaborationInfo,
        As4Payload payload)
    {
        Id = id;
        MessageId = messageId;
        ConversationId = conversationId;
        Sender = sender;
        Receiver = receiver;
        CollaborationInfo = collaborationInfo;
        Payload = payload;
        Status = As4MessageStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static As4Message Create(
        string messageId,
        string conversationId,
        Party sender,
        Party receiver,
        CollaborationInfo collaborationInfo,
        As4Payload payload)
    {
        if (string.IsNullOrWhiteSpace(messageId))
            throw new ArgumentException("Message id cannot be empty.", nameof(messageId));

        if (string.IsNullOrWhiteSpace(conversationId))
            throw new ArgumentException("Conversation id cannot be empty.", nameof(conversationId));

        return new As4Message(
            Guid.NewGuid(),
            messageId,
            conversationId,
            sender,
            receiver,
            collaborationInfo,
            payload);
    }

    public void MarkAsProcessing()
    {
        Status = As4MessageStatus.Processing;
        LastError = null;
    }

    public void MarkAsSent()
    {
        Status = As4MessageStatus.Sent;
        SentAtUtc = DateTime.UtcNow;
        LastError = null;
    }

    public void MarkAsFailed(string error)
    {
        Status = As4MessageStatus.Failed;
        LastError = error;
    }
}