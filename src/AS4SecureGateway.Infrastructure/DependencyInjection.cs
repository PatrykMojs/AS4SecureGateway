using AS4SecureGateway.Application.Abstractions.Certificates;
using AS4SecureGateway.Application.Abstractions.Compression;
using AS4SecureGateway.Application.Abstractions.Cryptography;
using AS4SecureGateway.Application.Abstractions.Messaging;
using AS4SecureGateway.Application.Abstractions.Responses;
using AS4SecureGateway.Application.Abstractions.Persistence;
using AS4SecureGateway.Infrastructure.Certificates;
using AS4SecureGateway.Infrastructure.Compression;
using AS4SecureGateway.Infrastructure.Cryptography.Encryption;
using AS4SecureGateway.Infrastructure.Cryptography.Signatures;
using AS4SecureGateway.Infrastructure.Http;
using AS4SecureGateway.Infrastructure.Responses;
using AS4SecureGateway.Infrastructure.Messaging;
using AS4SecureGateway.Infrastructure.Persistence;
using AS4SecureGateway.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AS4SecureGateway.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<As4CertificateOptions>(configuration.GetSection("Certificates"));
        
        services.Configure<As4TransportOptions>(
            configuration.GetSection("As4Transport"));

        services.AddHttpClient<IAs4TransportClient, As4TransportClient>();

        services.AddSingleton<IAs4CertificateProvider, As4CertificateProvider>();

        services.AddSingleton<IPayloadCompressor, GZipPayloadCompressor>();
        services.AddSingleton<IPayloadDecompressor, GZipPayloadDecompressor>();
        services.AddSingleton<SecuredAs4ResponseProcessor>();

        services.AddSingleton<IXmlBodySigner, XmlBodySigner>();
        services.AddSingleton<IXmlBodyEncryptor, XmlBodyEncryptor>();

        services.AddSingleton<IAttachmentSignatureBuilder, AttachmentSignatureBuilder>();
        services.AddSingleton<ICompressedAttachmentEncryptor, CompressedAttachmentEncryptor>();

        services.AddSingleton<IAs4BusinessBodyFactory, As4BusinessBodyFactory>();
        services.AddSingleton<IAs4EnvelopeFactory, As4EnvelopeFactory>();
        services.AddSingleton<IAs4SecurityPipeline, As4SecurityPipeline>();

        services.AddSingleton<IAs4ResponseParser, As4ResponseParser>();

        var connectionString = configuration.GetConnectionString("As4GatewayDatabase")
            ?? "Data Source=./data/as4-gateway.db";

        EnsureSqliteDirectoryExists(connectionString);

        services.AddDbContext<As4GatewayDbContext>(options =>
        {
            options.UseSqlite(connectionString);
        });

        services.AddScoped<IAs4MessageAuditRepository, SqliteAs4MessageAuditRepository>();

        return services;
    }

    private static void EnsureSqliteDirectoryExists(string connectionString)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString);

        if (string.IsNullOrWhiteSpace(builder.DataSource) ||
            builder.DataSource.Equals(":memory:", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var fullPath = Path.GetFullPath(builder.DataSource);
        var directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);
    }
}