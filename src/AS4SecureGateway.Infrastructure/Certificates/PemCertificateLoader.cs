using System.Security.Cryptography.X509Certificates;
using AS4SecureGateway.Infrastructure.Files;

namespace AS4SecureGateway.Infrastructure.Certificates;

public static class PemCertificateLoader
{
    public static X509Certificate2 LoadPublicCertificate(string path)
    {
        var fullPath = ResolvePath(path);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Public certificate file was not found: {fullPath}");

        var pem = File.ReadAllText(fullPath);

        if (!pem.Contains("BEGIN CERTIFICATE", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"File '{fullPath}' is not an X509 certificate. Expected '-----BEGIN CERTIFICATE-----'. " +
                "A raw public key file '-----BEGIN PUBLIC KEY-----' is not supported here.");
        }

        return X509Certificate2.CreateFromPem(pem);
    }

    public static X509Certificate2 LoadCertificateWithPrivateKey(string publicCertificatePath, string privateKeyPath)
    {
        var publicFullPath = ResolvePath(publicCertificatePath);
        var privateFullPath = ResolvePath(privateKeyPath);

        if (!File.Exists(publicFullPath))
            throw new FileNotFoundException($"Public certificate file was not found: {publicFullPath}");

        if (!File.Exists(privateFullPath))
            throw new FileNotFoundException($"Private key file was not found: {privateFullPath}");

        var certificatePem = File.ReadAllText(publicFullPath);
        var privateKeyPem = File.ReadAllText(privateFullPath);

        if (!certificatePem.Contains("BEGIN CERTIFICATE", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"File '{publicFullPath}' is not an X509 certificate. Expected '-----BEGIN CERTIFICATE-----'.");
        }

        if (!privateKeyPem.Contains("BEGIN PRIVATE KEY", StringComparison.OrdinalIgnoreCase) &&
            !privateKeyPem.Contains("BEGIN RSA PRIVATE KEY", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"File '{privateFullPath}' is not a supported PEM private key.");
        }

        var certificate = X509Certificate2.CreateFromPem(
            certificatePem,
            privateKeyPem);

        return new X509Certificate2(
            certificate.Export(X509ContentType.Pfx),
            (string?)null,
            X509KeyStorageFlags.Exportable);
    }

    private static string ResolvePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Certificate path cannot be empty.", nameof(path));

        if (Path.IsPathRooted(path))
            return path;

        var root = DirectoryFinder.FindAncestorNamed(
            AppContext.BaseDirectory,
            "AS4SecureGateway");

        return Path.GetFullPath(Path.Combine(root, path));
    }
}