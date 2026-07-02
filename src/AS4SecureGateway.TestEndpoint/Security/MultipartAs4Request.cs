namespace AS4SecureGateway.TestEndpoint.Security;

public sealed class MultipartAs4Request
{
    public string EnvelopeXml { get; init; } = string.Empty;
    public Dictionary<string, byte[]> Attachments { get; init; } = new();
}