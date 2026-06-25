namespace AS4SecureGateway.Infrastructure.Certificates;

public sealed class As4CertificateOptions
{
    public string RecipientEncryptionCertificatePath { get; init; } = string.Empty;
    public string SigningCertificatePath { get; init; } = string.Empty;
    public string SigningCertificatePassword { get; init; } = string.Empty;
    public string? SigningPublicCertificatePath { get; init; }
}