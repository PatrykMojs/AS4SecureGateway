using AS4SecureGateway.Application.Messaging;

namespace AS4SecureGateway.Application.UseCases.DispatchAs4Message;

public sealed class DispatchAs4MessageCommand
{
    public As4ActionType ActionType { get; init; }

    public string SenderPartyId { get; init; } = string.Empty;
    public string SenderRole { get; init; } = string.Empty;

    public string ReceiverPartyId { get; init; } = string.Empty;

    public string? PayloadXml { get; init; }
    public string? PeekMessageDomain { get; init; }
    public string? DocumentReferenceNumber { get; init; }

    public SecurityProcessingOptions SecurityOptions { get; init; } = new();
}