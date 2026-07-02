using System.Security.Cryptography.X509Certificates;

namespace AS4SecureGateway.TestEndpoint.Security;

public static class TestEndpointCertificateLoader
{
    public static X509Certificate2 LoadPublicCertificate(string path)
    {
        var fullPath = ResolvePath(path);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Public certificate file was not found: {fullPath}");

        var pem = File.ReadAllText(fullPath);

        if (!pem.Contains("BEGIN CERTIFICATE", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"File '{fullPath}' is not an X509 certificate PEM.");

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

        var certificate = X509Certificate2.CreateFromPem(certificatePem, privateKeyPem);

        return new X509Certificate2(certificate.Export(X509ContentType.Pfx), (string?)null, X509KeyStorageFlags.Exportable);
    }

    private static string ResolvePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Certificate path cannot be empty.", nameof(path));

        if (Path.IsPathRooted(path))
            return path;

        var root = FindAncestorNamed(AppContext.BaseDirectory, "AS4SecureGateway");

        return Path.GetFullPath(Path.Combine(root, path));
    }

    private static string FindAncestorNamed(string startPath, string folderName, int maxDepth = 15)
    {
        var directory = new DirectoryInfo(startPath);

        for (var i = 0; i < maxDepth && directory is not null; i++)
        {
            if (directory.Name.Equals(folderName, StringComparison.OrdinalIgnoreCase))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException($"Ancestor directory '{folderName}' was not found starting from '{startPath}'.");
    }
}