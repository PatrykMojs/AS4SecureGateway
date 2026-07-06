using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using AS4SecureGateway.TestEndpoint.Options;
using AS4SecureGateway.TestEndpoint.Responses;
using AS4SecureGateway.TestEndpoint.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AS4SecureGateway.Infrastructure.Tests.Integration.Support;

internal sealed class As4InMemoryTestEndpoint : IAsyncDisposable
{
    private readonly IHost _host;

    private As4InMemoryTestEndpoint(IHost host, HttpClient client)
    {
        _host = host;
        Client = client;
    }

    public HttpClient Client { get; }

    public static async Task<As4InMemoryTestEndpoint> StartAsync(IntegrationTestCertificates certificates)
    {
        var endpointCertificates = certificates.CreateEndpointCertificateOptions();

        var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.UseContentRoot(certificates.WorkDirectory);

                webBuilder.ConfigureAppConfiguration((_, configurationBuilder) =>
                {
                    configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Certificates:Local:PublicSignatureCertificatePath"] = endpointCertificates.Local.PublicSignatureCertificatePath,
                        ["Certificates:Local:PrivateSignatureCertificatePath"] = endpointCertificates.Local.PrivateSignatureCertificatePath,
                        ["Certificates:Local:PublicEncryptionCertificatePath"] = endpointCertificates.Local.PublicEncryptionCertificatePath,
                        ["Certificates:Local:PrivateEncryptionCertificatePath"] = endpointCertificates.Local.PrivateEncryptionCertificatePath,
                        ["Certificates:Remote:PublicSignatureCertificatePath"] = endpointCertificates.Remote.PublicSignatureCertificatePath,
                        ["Certificates:Remote:PublicEncryptionCertificatePath"] = endpointCertificates.Remote.PublicEncryptionCertificatePath
                    });
                });

                webBuilder.ConfigureServices((context, services) =>
                {
                    services.AddRouting();

                    services.Configure<TestEndpointCertificateOptions>(context.Configuration.GetSection("Certificates"));
                    services.AddSingleton<TestEndpointGZipDecompressor>();
                    services.AddSingleton<TestEndpointInboundSecurityProcessor>();
                    services.AddSingleton<TestEndpointResponseFactory>();
                });

                webBuilder.Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapGet("/", () => Results.Ok("AS4 Secure Gateway Test Endpoint is running."));
                        endpoints.MapPost("/api/as4/inbound", HandleInboundAsync);
                    });
                });
            })
            .StartAsync();

        return new As4InMemoryTestEndpoint(host, host.GetTestClient());
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _host.StopAsync();
        _host.Dispose();
    }

    private static async Task<IResult> HandleInboundAsync(
        HttpRequest request,
        IWebHostEnvironment environment,
        TestEndpointInboundSecurityProcessor securityProcessor,
        TestEndpointResponseFactory responseFactory,
        CancellationToken cancellationToken)
    {
        try
        {
            var receivedDirectory = Path.Combine(environment.ContentRootPath, "received");
            Directory.CreateDirectory(receivedDirectory);

            var contentType = request.ContentType ?? "unknown";
            var as4Request = await MultipartAs4RequestReader.ReadAsync(request, cancellationToken);
            var securedMessage = securityProcessor.Process(as4Request);
            var scenario = ExtractTestScenario(securedMessage.BusinessBodyXml);

            var fileName = $"as4-secured-request-{DateTime.UtcNow:yyyyMMdd-HHmmss-fff}.txt";
            var filePath = Path.Combine(receivedDirectory, fileName);
            var fileContent = $"""
                ReceivedAtUtc: {DateTime.UtcNow:O}
                Content-Type: {contentType}
                PayloadContentId: {securedMessage.PayloadContentId}
                SignatureValid: {securedMessage.SignatureValid}
                TestScenario: {scenario}

                DECRYPTED SOAP ENVELOPE:
                {securedMessage.DecryptedEnvelopeXml}

                BUSINESS BODY:
                {securedMessage.BusinessBodyXml}
                """;

            await File.WriteAllTextAsync(filePath, fileContent, Encoding.UTF8, cancellationToken);

            var response = responseFactory.CreateResponse(scenario, securedMessage.DecryptedEnvelopeXml, securedMessage.BusinessBodyXml);
            return Results.Content(response.Body, response.ContentType, Encoding.UTF8, response.StatusCode);
        }
        catch (Exception ex)
        {
            var response = responseFactory.CreateSecurityFaultResponse(ex.Message);
            return Results.Content(response.Body, response.ContentType, Encoding.UTF8, response.StatusCode);
        }
    }

    private static string ExtractTestScenario(string requestBody)
    {
        if (string.IsNullOrWhiteSpace(requestBody))
        {
            return "Ok";
        }

        try
        {
            var document = XDocument.Parse(requestBody);
            var scenario = document
                .Descendants()
                .FirstOrDefault(element => element.Name.LocalName == "TestScenario")
                ?.Value
                ?.Trim();

            if (!string.IsNullOrWhiteSpace(scenario))
            {
                return scenario;
            }
        }
        catch
        {
            // Fallback for raw content.
        }

        var match = Regex.Match(
            requestBody,
            @"<[^>]*TestScenario[^>]*>(.*?)</[^>]*TestScenario>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        return match.Success
            ? SecurityElement.Escape(match.Groups[1].Value.Trim()) ?? "Ok"
            : "Ok";
    }
}