namespace AS4SecureGateway.Infrastructure.Certificates;

public sealed class CertificateKeyMatchResult
{
    public bool Match { get; init; }
    public string Message { get; init; } = string.Empty;
}