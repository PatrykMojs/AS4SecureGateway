namespace AS4SecureGateway.Infrastructure.Responses;

public sealed class MultipartAs4Response
{
    public string EnvelopeXml { get; init; } = string.Empty;
    public Dictionary<string, byte[]> Attachments { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}