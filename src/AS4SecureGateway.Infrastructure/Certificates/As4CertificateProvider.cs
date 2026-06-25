using System.Security.Cryptography.X509Certificates;
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

        var recipientEncryptionCertificate = LoadPublicCertificate(_options.RecipientEncryptionCertificatePath);

        var signingCertificate  = CertificateInspector.LoadCertificateFromPfx(
            _options.SigningCertificatePath,
            _options.SigningCertificatePassword);

        var signingPublicCertificate = string.IsNullOrWhiteSpace(_options.SigningPublicCertificatePath)
            ? new X509Certificate2(signingCertificate.RawData)
            : LoadPublicCertificate(_options.SigningPublicCertificatePath);

        var certificateSet = new As4CertificateSet(
            recipientEncryptionCertificate,
            signingCertificate,
            signingPublicCertificate);

        return Task.FromResult(certificateSet);
    }

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(_options.RecipientEncryptionCertificatePath))
            throw new InvalidOperationException("Recipient encryption certificate path is not configured.");

        if (string.IsNullOrWhiteSpace(_options.SigningCertificatePath))
            throw new InvalidOperationException("Signing certificate path is not configured.");
    }

    private static X509Certificate2 LoadPublicCertificate(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Certificate path cannot be empty.", nameof(path));

        var extension = Path.GetExtension(path).ToLowerInvariant();

        return extension switch
        {
            ".pem" => CertificateInspector.LoadCertificateFromPem(path),
            ".cer" or ".crt" => new X509Certificate2(path),
            ".pfx" or ".p12" => new X509Certificate2(path),
            _ => new X509Certificate2(path)
        };
    }
}