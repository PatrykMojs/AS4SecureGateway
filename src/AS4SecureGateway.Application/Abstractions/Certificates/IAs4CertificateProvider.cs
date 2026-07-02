namespace AS4SecureGateway.Application.Abstractions.Certificates;

public interface IAs4CertificateProvider
{
    Task<As4CertificateSet> GetCertificatesAsync(CancellationToken cancellationToken);
}