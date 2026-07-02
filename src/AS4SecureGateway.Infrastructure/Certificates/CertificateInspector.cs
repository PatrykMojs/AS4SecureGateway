using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace AS4SecureGateway.Infrastructure.Certificates;

public static class CertificateInspector
{
    public static X509Certificate2 LoadCertificateFromPem(string certificatePath)
    {
        if (string.IsNullOrWhiteSpace(certificatePath))
            throw new ArgumentException("Certificate path cannot be empty.", nameof(certificatePath));

        if (!File.Exists(certificatePath))
            throw new FileNotFoundException("Certificate file was not found.", certificatePath);

        var pemContent = File.ReadAllText(certificatePath);

        var base64 = pemContent
            .Replace("-----BEGIN CERTIFICATE-----", string.Empty)
            .Replace("-----END CERTIFICATE-----", string.Empty)
            .Replace("\r", string.Empty)
            .Replace("\n", string.Empty)
            .Trim();

        var rawData = Convert.FromBase64String(base64);

        return new X509Certificate2(rawData);
    }

    public static X509Certificate2 LoadCertificateFromPfx(
        string certificatePath,
        string password)
    {
        if (string.IsNullOrWhiteSpace(certificatePath))
            throw new ArgumentException("Certificate path cannot be empty.", nameof(certificatePath));

        if (!File.Exists(certificatePath))
            throw new FileNotFoundException("Certificate file was not found.", certificatePath);

        return new X509Certificate2(
            certificatePath,
            password,
            X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet);
    }

    public static CertificateThumbprints GetThumbprints(X509Certificate2 certificate)
    {
        ArgumentNullException.ThrowIfNull(certificate);

        using var sha256 = SHA256.Create();

        var sha256Thumbprint = Convert.ToHexString(
            sha256.ComputeHash(certificate.RawData));

        return new CertificateThumbprints
        {
            Sha1 = certificate.Thumbprint?.ToLowerInvariant() ?? string.Empty,
            Sha256 = sha256Thumbprint.ToLowerInvariant()
        };
    }

    public static CertificateKeyMatchResult ValidateCertificatesMatch(
        X509Certificate2 publicKeyCertificate,
        X509Certificate2 privateKeyCertificate)
    {
        ArgumentNullException.ThrowIfNull(publicKeyCertificate);
        ArgumentNullException.ThrowIfNull(privateKeyCertificate);

        using var publicKey = publicKeyCertificate.GetRSAPublicKey();
        using var privateKey = privateKeyCertificate.GetRSAPrivateKey();

        if (publicKey is null)
        {
            return new CertificateKeyMatchResult
            {
                Match = false,
                Message = "Public certificate does not contain an RSA public key."
            };
        }

        if (privateKey is null)
        {
            return new CertificateKeyMatchResult
            {
                Match = false,
                Message = "Private certificate does not contain an RSA private key."
            };
        }

        var publicParameters = publicKey.ExportParameters(false);
        var privateParameters = privateKey.ExportParameters(false);

        var match =
            publicParameters.Modulus is not null &&
            privateParameters.Modulus is not null &&
            publicParameters.Exponent is not null &&
            privateParameters.Exponent is not null &&
            publicParameters.Modulus.SequenceEqual(privateParameters.Modulus) &&
            publicParameters.Exponent.SequenceEqual(privateParameters.Exponent);

        return new CertificateKeyMatchResult
        {
            Match = match,
            Message = match
                ? "Public and private keys match."
                : "Public and private keys do not match."
        };
    }
}