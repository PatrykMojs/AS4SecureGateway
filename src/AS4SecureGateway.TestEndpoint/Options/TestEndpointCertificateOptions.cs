namespace AS4SecureGateway.TestEndpoint.Options;

public sealed class TestEndpointCertificateOptions
{
    public TestEndpointLocalCertificateOptions Local { get; init; } = new();
    public TestEndpointRemoteCertificateOptions Remote { get; init; } = new();
}

public sealed class TestEndpointLocalCertificateOptions
{
    public string PublicSignatureCertificatePath { get; init; } = string.Empty;
    public string PrivateSignatureCertificatePath { get; init; } = string.Empty;
    public string PublicEncryptionCertificatePath { get; init; } = string.Empty;
    public string PrivateEncryptionCertificatePath { get; init; } = string.Empty;
}

public sealed class TestEndpointRemoteCertificateOptions
{
    public string PublicSignatureCertificatePath { get; init; } = string.Empty;
    public string PublicEncryptionCertificatePath { get; init; } = string.Empty;
}