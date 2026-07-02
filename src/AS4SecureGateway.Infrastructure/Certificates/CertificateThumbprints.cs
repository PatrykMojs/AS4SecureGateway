namespace AS4SecureGateway.Infrastructure.Certificates;

public sealed class CertificateThumbprints
{
    public string Sha1 { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
}