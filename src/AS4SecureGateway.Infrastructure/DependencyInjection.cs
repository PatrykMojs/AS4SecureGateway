using AS4SecureGateway.Application.Abstractions.Certificates;
using AS4SecureGateway.Application.Abstractions.Compression;
using AS4SecureGateway.Application.Abstractions.Cryptography;
using AS4SecureGateway.Application.Abstractions.Messaging;
using AS4SecureGateway.Infrastructure.Certificates;
using AS4SecureGateway.Infrastructure.Compression;
using AS4SecureGateway.Infrastructure.Cryptography.Encryption;
using AS4SecureGateway.Infrastructure.Cryptography.Signatures;
using AS4SecureGateway.Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AS4SecureGateway.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<As4CertificateOptions>(configuration.GetSection("Certificates"));

        services.AddSingleton<IAs4CertificateProvider, As4CertificateProvider>();

        services.AddSingleton<IPayloadCompressor, GZipPayloadCompressor>();

        services.AddSingleton<IXmlBodySigner, XmlBodySigner>();
        services.AddSingleton<IXmlBodyEncryptor, XmlBodyEncryptor>();

        services.AddSingleton<IAttachmentSignatureBuilder, AttachmentSignatureBuilder>();
        services.AddSingleton<ICompressedAttachmentEncryptor, CompressedAttachmentEncryptor>();

        services.AddSingleton<IAs4BusinessBodyFactory, As4BusinessBodyFactory>();
        services.AddSingleton<IAs4EnvelopeFactory, As4EnvelopeFactory>();
        services.AddSingleton<IAs4SecurityPipeline, As4SecurityPipeline>();

        return services;
    }
}