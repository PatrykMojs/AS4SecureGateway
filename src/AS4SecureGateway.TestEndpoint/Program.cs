using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using AS4SecureGateway.TestEndpoint.Options;
using AS4SecureGateway.TestEndpoint.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<TestEndpointCertificateOptions>(
    builder.Configuration.GetSection("Certificates"));

builder.Services.AddSingleton<TestEndpointGZipDecompressor>();
builder.Services.AddSingleton<TestEndpointInboundSecurityProcessor>();

var app = builder.Build();

app.MapGet("/", () => Results.Ok("AS4 Secure Gateway Test Endpoint is running."));

app.MapPost("/api/as4/inbound", async (
    HttpRequest request,
    IWebHostEnvironment environment,
    ILogger<Program> logger,
    TestEndpointInboundSecurityProcessor securityProcessor,
    CancellationToken cancellationToken) =>
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

        logger.LogInformation(
            "Secured AS4 request processed. SignatureValid = {SignatureValid}, Scenario = {Scenario}",
            securedMessage.SignatureValid,
            scenario);

        var (statusCode, responseXml) = CreateResponse(scenario);

        return Results.Content(responseXml, "application/soap+xml", Encoding.UTF8, statusCode);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to process secured AS4 request.");

        var responseXml = CreateSecurityFaultResponse(ex.Message);

        return Results.Content(responseXml, "application/soap+xml", Encoding.UTF8, StatusCodes.Status400BadRequest);
    }
});

app.Run();

static string ExtractTestScenario(string requestBody)
{
    if (string.IsNullOrWhiteSpace(requestBody))
        return "Ok";

    try
    {
        var document = XDocument.Parse(requestBody);

        var scenario = document
            .Descendants()
            .FirstOrDefault(element => element.Name.LocalName == "TestScenario")
            ?.Value
            ?.Trim();

        if (!string.IsNullOrWhiteSpace(scenario))
            return scenario;
    }
    catch
    {
        // Fallback for raw or multipart-like content.
    }

    var match = Regex.Match(
        requestBody,
        @"<[^>]*TestScenario[^>]*>(.*?)</[^>]*TestScenario>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline);

    return match.Success
        ? match.Groups[1].Value.Trim()
        : "Ok";
}

static (int StatusCode, string ResponseXml) CreateResponse(string scenario)
{
    return scenario switch
    {
        "Ok" => CreateAccepted202Response(),
        "BadRequest" => CreateBadRequestResponse(),
        "NotFound" => CreateNotFoundResponse(),
        _ => CreateAccepted200Response()
    };
}

static (int StatusCode, string ResponseXml) CreateAccepted200Response()
{
    var responseXml = $"""
                      <soap:Envelope xmlns:soap="http://www.w3.org/2003/05/soap-envelope"
                                     xmlns:demo="urn:demo:as4:test-endpoint:v1">
                        <soap:Body>
                          <demo:SendMessageResponse>
                            <demo:Status>Accepted</demo:Status>
                            <demo:HttpStatusCode>200</demo:HttpStatusCode>
                            <demo:MessageId>{Guid.NewGuid()}@test-endpoint</demo:MessageId>
                            <demo:DocumentId>DOC-2026-0001</demo:DocumentId>
                            <demo:ReceivedAtUtc>{DateTime.UtcNow:O}</demo:ReceivedAtUtc>
                          </demo:SendMessageResponse>
                        </soap:Body>
                      </soap:Envelope>
                      """;

    return (StatusCodes.Status200OK, responseXml);
}

static (int StatusCode, string ResponseXml) CreateAccepted202Response()
{
    var responseXml = $"""
                      <soap:Envelope xmlns:soap="http://www.w3.org/2003/05/soap-envelope"
                                     xmlns:demo="urn:demo:as4:test-endpoint:v1">
                        <soap:Body>
                          <demo:SendMessageResponse>
                            <demo:Status>Queued</demo:Status>
                            <demo:HttpStatusCode>202</demo:HttpStatusCode>
                            <demo:MessageId>{Guid.NewGuid()}@test-endpoint</demo:MessageId>
                            <demo:DocumentId>DOC-2026-0001</demo:DocumentId>
                            <demo:EstimatedProcessingTimeSeconds>30</demo:EstimatedProcessingTimeSeconds>
                            <demo:ReceivedAtUtc>{DateTime.UtcNow:O}</demo:ReceivedAtUtc>
                          </demo:SendMessageResponse>
                        </soap:Body>
                      </soap:Envelope>
                      """;

    return (StatusCodes.Status202Accepted, responseXml);
}

static (int StatusCode, string ResponseXml) CreateBadRequestResponse()
{
    var responseXml = """
                      <soap:Envelope xmlns:soap="http://www.w3.org/2003/05/soap-envelope"
                                     xmlns:demo="urn:demo:as4:test-endpoint:v1">
                        <soap:Body>
                          <soap:Fault>
                            <soap:Code>
                              <soap:Value>soap:Sender</soap:Value>
                            </soap:Code>
                            <soap:Reason>
                              <soap:Text xml:lang="en">Invalid demo AS4 message.</soap:Text>
                            </soap:Reason>
                            <soap:Detail>
                              <demo:Error>
                                <demo:ErrorCode>DEMO-400</demo:ErrorCode>
                                <demo:Description>Required business document data is missing or invalid.</demo:Description>
                              </demo:Error>
                            </soap:Detail>
                          </soap:Fault>
                        </soap:Body>
                      </soap:Envelope>
                      """;

    return (StatusCodes.Status400BadRequest, responseXml);
}

static (int StatusCode, string ResponseXml) CreateNotFoundResponse()
{
    var responseXml = """
                      <soap:Envelope xmlns:soap="http://www.w3.org/2003/05/soap-envelope"
                                     xmlns:demo="urn:demo:as4:test-endpoint:v1">
                        <soap:Body>
                          <soap:Fault>
                            <soap:Code>
                              <soap:Value>soap:Sender</soap:Value>
                            </soap:Code>
                            <soap:Reason>
                              <soap:Text xml:lang="en">Document was not found.</soap:Text>
                            </soap:Reason>
                            <soap:Detail>
                              <demo:Error>
                                <demo:ErrorCode>DEMO-404</demo:ErrorCode>
                                <demo:Description>The requested document or message reference does not exist.</demo:Description>
                              </demo:Error>
                            </soap:Detail>
                          </soap:Fault>
                        </soap:Body>
                      </soap:Envelope>
                      """;

    return (StatusCodes.Status404NotFound, responseXml);
}

static string CreateSecurityFaultResponse(string message)
{
    var safeMessage = SecurityElement.Escape(message) ?? "Security processing failed.";

    return $"""
           <soap:Envelope xmlns:soap="http://www.w3.org/2003/05/soap-envelope"
                          xmlns:demo="urn:demo:as4:test-endpoint:v1">
             <soap:Body>
               <soap:Fault>
                 <soap:Code>
                   <soap:Value>soap:Sender</soap:Value>
                 </soap:Code>
                 <soap:Reason>
                   <soap:Text xml:lang="en">AS4 security processing failed.</soap:Text>
                 </soap:Reason>
                 <soap:Detail>
                   <demo:Error>
                     <demo:ErrorCode>DEMO-SECURITY-400</demo:ErrorCode>
                     <demo:Description>{safeMessage}</demo:Description>
                   </demo:Error>
                 </soap:Detail>
               </soap:Fault>
             </soap:Body>
           </soap:Envelope>
           """;
}