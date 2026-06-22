namespace AS4SecureGateway.Application.Messaging;

public sealed class As4MessageMetadata
{
    public string SenderPartyId { get; init; } = string.Empty;
    public string SenderRole { get; init; } = string.Empty;

    public string ReceiverPartyId { get; init; } = string.Empty;
    public string ReceiverRole { get; init; } = string.Empty;

    public string AgreementRef { get; init; } = string.Empty;
    public string Service { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string ConversationId { get; init; } = string.Empty;

    public string? MimeType { get; init; }
    public string? CharacterSet { get; init; }
    public string? CompressionType { get; init; }
}