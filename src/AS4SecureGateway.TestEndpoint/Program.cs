using System.Text;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => Results.Ok("AS4 Secure Gateway Test Endpoint is runing."));

app.MapPost("/api/as4/inbound", async (
    HttpRequest request,
    IWebHostEnvironment environment,
    ILogger<Program> logger,
    CancellationToken cancellationToken) =>
{
   var receivedDirectory = Path.Combine(environment.ContentRootPath, "received");

   Directory.CreateDirectory(receivedDirectory);

   var contentType = request.ContentType ?? "unknown";

   using var reader = new StreamReader(
    request.Body,
    Encoding.UTF8,
    detectEncodingFromByteOrderMarks: true,
    leaveOpen: false);

    var requestBody = await reader.ReadToEndAsync(cancellationToken);

    var fileName = $"as4-request-{DateTime.UtcNow:yyyyMMdd-HHmmss-fff}.txt";
    var filePath = Path.Combine(receivedDirectory, fileName);

    var fileContent = $"""
                      ReceivedAtUtc: {DateTime.UtcNow:O}
                      Content-Type: {contentType}

                      {requestBody}
                      """; 

    await File.WriteAllTextAsync(filePath, fileContent, Encoding.UTF8, cancellationToken);

    logger.LogInformation(
        "Received AS4 message. Content-Type: {ContentType}. Saved to: {FilePath}",
        contentType,
        filePath);

    var responseXml = $"""
                      <soap:Envelope xmlns:soap="http://www.w3.org/2003/05/soap-envelope">
                        <soap:Body>
                          <SubmitMessageResponse xmlns="urn:demo:as4:test-endpoint">
                            <Status>Accepted</Status>
                            <MessageId>{Guid.NewGuid()}</MessageId>
                            <ReceivedAtUtc>{DateTime.UtcNow:O}</ReceivedAtUtc>
                          </SubmitMessageResponse>
                        </soap:Body>
                      </soap:Envelope>
                      """; 

    return Results.Content(
        responseXml,
        "application/soap+xml",
        Encoding.UTF8);
});

app.Run();
