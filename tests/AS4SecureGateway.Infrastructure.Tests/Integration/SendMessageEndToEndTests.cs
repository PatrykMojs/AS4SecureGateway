using System.Net;
using AS4SecureGateway.Application.Messaging;
using AS4SecureGateway.Application.UseCases.DispatchAs4Message;
using AS4SecureGateway.Infrastructure.Certificates;
using AS4SecureGateway.Infrastructure.Compression;
using AS4SecureGateway.Infrastructure.Cryptography.Encryption;
using AS4SecureGateway.Infrastructure.Cryptography.Signatures;
using AS4SecureGateway.Infrastructure.Http;
using AS4SecureGateway.Infrastructure.Messaging;
using AS4SecureGateway.Infrastructure.Responses;
using AS4SecureGateway.Infrastructure.Tests.Integration.Support;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace AS4SecureGateway.Infrastructure.Tests.Integration;

public sealed class SendMessageEndToEndTests
{
    [Fact]
    public async Task SendMessage_WithCompressionSignatureAndEncryption_ShouldReachTestEndpointAndReturnAcceptedResponse()
    {
        using var certificates = IntegrationTestCertificates.Create();
        await using var testEndpoint = await As4InMemoryTestEndpoint.StartAsync(certificates);

        var auditRepository = new InMemoryAs4MessageAuditRepository();
        var handler = CreateHandler(testEndpoint.Client, certificates, auditRepository);

        var result = await handler.HandleAsync(new DispatchAs4MessageCommand
        {
            ActionType = As4ActionType.SendMessage,
            SenderPartyId = "client-party",
            SenderRole = "http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/200704/initiator",
            ReceiverPartyId = "server-party",
            PayloadXml = """
                <demo:Invoice xmlns:demo="urn:demo:invoice:v1">
                    <demo:DocumentId>DOC-E2E-0001</demo:DocumentId>
                    <demo:Amount currency="PLN">123.45</demo:Amount>
                </demo:Invoice>
                """,
            TestScenario = As4TestScenario.Accepted,
            SecurityOptions = new SecurityProcessingOptions
            {
                EnableCompression = true,
                EnableSignature = true,
                EnableEncryption = true
            }
        }, CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.IsSuccessStatusCode.Should().BeTrue();
        result.ResponseBody.Should().Contain("SendMessageResponse");
        result.ResponseBody.Should().Contain("DOC-E2E-0001");

        result.ParsedResponse.Should().NotBeNull();
        result.ParsedResponse!.HasFault.Should().BeFalse();
        result.ParsedResponse.Status.Should().Be("Accepted");
        result.ParsedResponse.DocumentId.Should().Be("DOC-E2E-0001");
        result.ParsedResponse.ReceivedAtUtc.Should().NotBeNullOrWhiteSpace();

        var auditRecord = auditRepository.Records.Should().ContainSingle().Subject;
        auditRecord.IsSuccess.Should().BeTrue();
        auditRecord.HttpStatusCode.Should().Be(200);
        auditRecord.Status.Should().Be("Accepted");
        auditRecord.DocumentId.Should().Be("DOC-E2E-0001");
        auditRecord.RequestXml.Should().NotBeNullOrWhiteSpace();
        auditRecord.ResponseBody.Should().Contain("SendMessageResponse");

        var receivedDirectory = Path.Combine(certificates.WorkDirectory, "received");
        Directory.Exists(receivedDirectory).Should().BeTrue();

        var receivedFile = Directory.GetFiles(receivedDirectory, "as4-secured-request-*.txt")
            .Should()
            .ContainSingle()
            .Subject;

        var receivedContent = await File.ReadAllTextAsync(receivedFile);

        receivedContent.Should().Contain("SignatureValid: True");
        receivedContent.Should().Contain("TestScenario: Accepted");
        receivedContent.Should().Contain("DOC-E2E-0001");
    }

    private static DispatchAs4MessageHandler CreateHandler(
        HttpClient httpClient,
        IntegrationTestCertificates certificates,
        InMemoryAs4MessageAuditRepository auditRepository)
    {
        var certificateProvider = new As4CertificateProvider(
            Options.Create(certificates.CreateClientCertificateOptions()));

        var securityPipeline = new As4SecurityPipeline(
            new GZipPayloadCompressor(),
            new XmlBodySigner(),
            new XmlBodyEncryptor(),
            new AttachmentSignatureBuilder(),
            new CompressedAttachmentEncryptor(),
            certificateProvider);

        var securedResponseProcessor = new SecuredAs4ResponseProcessor(
            new TestGZipPayloadDecompressor(),
            Options.Create(certificates.CreateClientCertificateOptions()));

        var transportClient = new As4TransportClient(
            httpClient,
            Options.Create(new As4TransportOptions
            {
                EndpointUrl = "/api/as4/inbound",
                TimeoutSeconds = 30
            }),
            securedResponseProcessor);

        return new DispatchAs4MessageHandler(
            new As4MessageMetadataFactory(),
            new As4BusinessBodyFactory(),
            new As4EnvelopeFactory(),
            securityPipeline,
            transportClient,
            new As4ResponseParser(),
            auditRepository);
    }
}