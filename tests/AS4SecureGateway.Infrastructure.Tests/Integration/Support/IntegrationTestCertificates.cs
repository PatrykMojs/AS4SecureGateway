using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using AS4SecureGateway.Infrastructure.Certificates;
using TestEndpointOptions = AS4SecureGateway.TestEndpoint.Options.TestEndpointCertificateOptions;
using TestEndpointLocalOptions = AS4SecureGateway.TestEndpoint.Options.TestEndpointLocalCertificateOptions;
using TestEndpointRemoteOptions = AS4SecureGateway.TestEndpoint.Options.TestEndpointRemoteCertificateOptions;

namespace AS4SecureGateway.Infrastructure.Tests.Integration.Support;

internal sealed class IntegrationTestCertificates : IDisposable
{
    private IntegrationTestCertificates(string workDirectory)
    {
        WorkDirectory = workDirectory;
    }

    public string WorkDirectory { get; }

    public string ClientSigningPublicCertificatePath { get; private init; } = string.Empty;
    public string ClientSigningPrivateKeyPath { get; private init; } = string.Empty;
    public string ClientEncryptionPublicCertificatePath { get; private init; } = string.Empty;
    public string ClientEncryptionPrivateKeyPath { get; private init; } = string.Empty;

    public string ServerSigningPublicCertificatePath { get; private init; } = string.Empty;
    public string ServerSigningPrivateKeyPath { get; private init; } = string.Empty;
    public string ServerEncryptionPublicCertificatePath { get; private init; } = string.Empty;
    public string ServerEncryptionPrivateKeyPath { get; private init; } = string.Empty;

    public static IntegrationTestCertificates Create()
    {
        var workDirectory = Path.Combine(Path.GetTempPath(), "as4-e2e-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workDirectory);

        var clientSigning = CreateCertificate(workDirectory, "client-signing", X509KeyUsageFlags.DigitalSignature);
        var clientEncryption = CreateCertificate(workDirectory, "client-encryption", X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DataEncipherment);
        var serverSigning = CreateCertificate(workDirectory, "server-signing", X509KeyUsageFlags.DigitalSignature);
        var serverEncryption = CreateCertificate(workDirectory, "server-encryption", X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DataEncipherment);

        return new IntegrationTestCertificates(workDirectory)
        {
            ClientSigningPublicCertificatePath = clientSigning.PublicCertificatePath,
            ClientSigningPrivateKeyPath = clientSigning.PrivateKeyPath,
            ClientEncryptionPublicCertificatePath = clientEncryption.PublicCertificatePath,
            ClientEncryptionPrivateKeyPath = clientEncryption.PrivateKeyPath,

            ServerSigningPublicCertificatePath = serverSigning.PublicCertificatePath,
            ServerSigningPrivateKeyPath = serverSigning.PrivateKeyPath,
            ServerEncryptionPublicCertificatePath = serverEncryption.PublicCertificatePath,
            ServerEncryptionPrivateKeyPath = serverEncryption.PrivateKeyPath
        };
    }

    public As4CertificateOptions CreateClientCertificateOptions()
    {
        return new As4CertificateOptions
        {
            Local = new CertificateSideOptions
            {
                PublicSignatureCertificatePath = ClientSigningPublicCertificatePath,
                PrivateSignatureCertificatePath = ClientSigningPrivateKeyPath,
                PublicEncryptionCertificatePath = ClientEncryptionPublicCertificatePath,
                PrivateEncryptionCertificatePath = ClientEncryptionPrivateKeyPath
            },
            Remote = new RemoteCertificateOptions
            {
                PublicSignatureCertificatePath = ServerSigningPublicCertificatePath,
                PublicEncryptionCertificatePath = ServerEncryptionPublicCertificatePath
            }
        };
    }

    public TestEndpointOptions CreateEndpointCertificateOptions()
    {
        return new TestEndpointOptions
        {
            Local = new TestEndpointLocalOptions
            {
                PublicSignatureCertificatePath = ServerSigningPublicCertificatePath,
                PrivateSignatureCertificatePath = ServerSigningPrivateKeyPath,
                PublicEncryptionCertificatePath = ServerEncryptionPublicCertificatePath,
                PrivateEncryptionCertificatePath = ServerEncryptionPrivateKeyPath
            },
            Remote = new TestEndpointRemoteOptions
            {
                PublicSignatureCertificatePath = ClientSigningPublicCertificatePath,
                PublicEncryptionCertificatePath = ClientEncryptionPublicCertificatePath
            }
        };
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(WorkDirectory))
            {
                Directory.Delete(WorkDirectory, recursive: true);
            }
        }
        catch
        {
            // Cleanup errors must not hide the real test result.
        }
    }

    private static CertificateFiles CreateCertificate(string directory, string name, X509KeyUsageFlags keyUsageFlags)
    {
        using var rsa = RSA.Create(2048);

        var request = new CertificateRequest(
            $"CN=AS4 integration test {name}",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        request.CertificateExtensions.Add(new X509KeyUsageExtension(keyUsageFlags, critical: true));
        request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, critical: false));

        using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));

        var publicCertificatePath = Path.Combine(directory, $"{name}.cert.pem");
        var privateKeyPath = Path.Combine(directory, $"{name}.key.pem");

        File.WriteAllText(publicCertificatePath, certificate.ExportCertificatePem());
        File.WriteAllText(privateKeyPath, rsa.ExportPkcs8PrivateKeyPem());

        return new CertificateFiles(publicCertificatePath, privateKeyPath);
    }

    private sealed record CertificateFiles(string PublicCertificatePath, string PrivateKeyPath);
}
