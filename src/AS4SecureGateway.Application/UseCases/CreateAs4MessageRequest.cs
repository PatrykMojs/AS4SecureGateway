namespace AS4SecureGateway.Application.UseCases.CreateAs4Message;

public sealed class CreateAs4MessageRequest
{
    public string SenderPartyId { get; init; } = string.Empty;
    public string SenderRole { get; init; } = string.Empty;

    public string ReceiverPartyId { get; init; } = string.Empty;
    public string ReceiverRole { get; init; } = string.Empty;

    public string AgreementRef { get; init; } = string.Empty;
    public string Service { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;

    public string? ConversationId { get; init; }

    public string MimeType { get; init; } = "application/xml";
    public string CharacterSet { get; init; } = "UTF-8";
    public string? CompressionType { get; init; } = "application/gzip";

    public byte[] PayloadContent { get; init; } = [];
}