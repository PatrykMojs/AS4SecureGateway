using System.Security.Cryptography.X509Certificates;

namespace AS4SecureGateway.Application.Abstractions.Certificates;

public sealed record As4CertificateSet(
    X509Certificate2 LocalPublicSignatureCertificate,
    X509Certificate2 LocalPrivateSignatureCertificate,
    X509Certificate2 LocalPublicEncryptionCertificate,
    X509Certificate2 LocalPrivateEncryptionCertificate,
    X509Certificate2 RemotePublicSignatureCertificate,
    X509Certificate2 RemotePublicEncryptionCertificate);