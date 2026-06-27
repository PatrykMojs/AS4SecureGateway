using AS4SecureGateway.Application.Messaging;

namespace AS4SecureGateway.WorkerService.Options;

public sealed class As4WorkerOptions
{
    public string SenderPartyId { get; init; } = string.Empty;
    public string SenderRole { get; init; } = string.Empty;
    public string ReceiverPartyId { get; init; } = string.Empty;

    public string DemoPayloadXml { get; init; } = string.Empty;

    public string? PeekMessageDomain { get; init; }
    public string? DocumentReferenceNumber { get; init; }

    public bool EnableCompression { get; init; }
    public bool EnableEncryption { get; init; }
    public bool EnableSignature { get; init; }

    public SecurityProcessingOptions ToSecurityOptions()
    {
        return new SecurityProcessingOptions
        {
            EnableCompression = EnableCompression,
            EnableEncryption = EnableEncryption,
            EnableSignature = EnableSignature
        };
    }
}