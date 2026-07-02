namespace AS4SecureGateway.Infrastructure.Certificates;

public sealed class As4CertificateOptions
{
    public CertificateSideOptions Local { get; init; } = new();
    public RemoteCertificateOptions Remote { get; init; } = new();
}

public sealed class CertificateSideOptions
{
    public string PublicSignatureCertificatePath { get; init; } = string.Empty;
    public string PrivateSignatureCertificatePath { get; init; } = string.Empty;
    public string PublicEncryptionCertificatePath { get; init; } = string.Empty;
    public string PrivateEncryptionCertificatePath { get; init; } = string.Empty;
}

public sealed class RemoteCertificateOptions
{
    public string PublicSignatureCertificatePath { get; init; } = string.Empty;
    public string PublicEncryptionCertificatePath { get; init; } = string.Empty;
}