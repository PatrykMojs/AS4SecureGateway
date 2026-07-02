namespace AS4SecureGateway.TestEndpoint.Security;

public sealed class SecuredInboundAs4Message
{
    public string DecryptedEnvelopeXml { get; init; } = string.Empty;
    public string BusinessBodyXml { get; init; } = string.Empty;
    public string PayloadContentId { get; init; } = string.Empty;
    public bool SignatureValid { get; init; }
}