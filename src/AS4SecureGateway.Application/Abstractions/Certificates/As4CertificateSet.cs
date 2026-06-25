using System.Security.Cryptography.X509Certificates;

namespace AS4SecureGateway.Application.Abstractions.Certificates;

public sealed record As4CertificateSet(
    X509Certificate2 RecipientEncryptionCertificate,
    X509Certificate2 SigningCertificate,
    X509Certificate2 SigningPublicCertificate);