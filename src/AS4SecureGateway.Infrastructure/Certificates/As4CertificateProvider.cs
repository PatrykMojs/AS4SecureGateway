using AS4SecureGateway.Application.Abstractions.Certificates;
using Microsoft.Extensions.Options;

namespace AS4SecureGateway.Infrastructure.Certificates;

public sealed class As4CertificateProvider : IAs4CertificateProvider
{
    private readonly As4CertificateOptions _options;

    public As4CertificateProvider(IOptions<As4CertificateOptions> options)
    {
        _options = options.Value;
    }

    public Task<As4CertificateSet> GetCertificatesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ValidateOptions();

        var localPublicSignatureCertificate = PemCertificateLoader.LoadPublicCertificate(
            _options.Local.PublicSignatureCertificatePath);

        var localPrivateSignatureCertificate = PemCertificateLoader.LoadCertificateWithPrivateKey(
            _options.Local.PublicSignatureCertificatePath,
            _options.Local.PrivateSignatureCertificatePath);

        var localPublicEncryptionCertificate = PemCertificateLoader.LoadPublicCertificate(
            _options.Local.PublicEncryptionCertificatePath);

        var localPrivateEncryptionCertificate = PemCertificateLoader.LoadCertificateWithPrivateKey(
            _options.Local.PublicEncryptionCertificatePath,
            _options.Local.PrivateEncryptionCertificatePath);

        var remotePublicSignatureCertificate = PemCertificateLoader.LoadPublicCertificate(
            _options.Remote.PublicSignatureCertificatePath);

        var remotePublicEncryptionCertificate = PemCertificateLoader.LoadPublicCertificate(
            _options.Remote.PublicEncryptionCertificatePath);

        var certificateSet = new As4CertificateSet(
            localPublicSignatureCertificate,
            localPrivateSignatureCertificate,
            localPublicEncryptionCertificate,
            localPrivateEncryptionCertificate,
            remotePublicSignatureCertificate,
            remotePublicEncryptionCertificate);

        return Task.FromResult(certificateSet);
    }

    private void ValidateOptions()
    {
        ValidatePath(_options.Local.PublicSignatureCertificatePath, "Certificates:Local:PublicSignatureCertificatePath");
        ValidatePath(_options.Local.PrivateSignatureCertificatePath, "Certificates:Local:PrivateSignatureCertificatePath");
        ValidatePath(_options.Local.PublicEncryptionCertificatePath, "Certificates:Local:PublicEncryptionCertificatePath");
        ValidatePath(_options.Local.PrivateEncryptionCertificatePath, "Certificates:Local:PrivateEncryptionCertificatePath");

        ValidatePath(_options.Remote.PublicSignatureCertificatePath, "Certificates:Remote:PublicSignatureCertificatePath");
        ValidatePath(_options.Remote.PublicEncryptionCertificatePath, "Certificates:Remote:PublicEncryptionCertificatePath");
    }

    private static void ValidatePath(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"{name} is required.");
    }
}