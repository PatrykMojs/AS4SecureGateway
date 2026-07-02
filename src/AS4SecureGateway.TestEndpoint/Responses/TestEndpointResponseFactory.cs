using System.Security;
using System.Xml.Linq;

namespace AS4SecureGateway.TestEndpoint.Responses;

public sealed class TestEndpointResponseFactory
{
    private const string SoapNamespace = "http://www.w3.org/2003/05/soap-envelope";
    private const string DemoNamespace = "urn:demo:as4:test-endpoint:v1";

    public TestEndpointSoapResponse CreateResponse(string scenario, string decryptedEnvelopeXml, string businessBodyXml)
    {
        var requestMessageId = ExtractValue(decryptedEnvelopeXml, "MessageId")
            ?? $"{Guid.NewGuid()}@unknown-request";

        var documentId = ExtractValue(businessBodyXml, "DocumentId")
            ?? "DOC-UNKNOWN";

        return scenario switch
        {
            "Accepted202" => CreateAccepted202Response(requestMessageId, documentId),
            "BadRequest" => CreateBadRequestResponse(requestMessageId),
            "NotFound" => CreateNotFoundResponse(requestMessageId, documentId),
            _ => CreateAccepted200Response(requestMessageId, documentId)
        };
    }

    public TestEndpointSoapResponse CreateSecurityFaultResponse(string message)
    {
        var safeMessage = SecurityElement.Escape(message) ?? "Security processing failed.";
        var responseXml = $"""
                          <soap:Envelope xmlns:soap="{SoapNamespace}"
                                         xmlns:demo="{DemoNamespace}">
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

        return new TestEndpointSoapResponse(StatusCodes.Status400BadRequest, "application/soap+xml", responseXml);
    }

    private static TestEndpointSoapResponse CreateAccepted200Response(string requestMessageId, string documentId)
    {
        var responseMessageId = $"{Guid.NewGuid()}@test-endpoint";
        var responseXml = $"""
                          <soap:Envelope xmlns:soap="{SoapNamespace}"
                                         xmlns:demo="{DemoNamespace}">
                            <soap:Body>
                              <demo:SendMessageResponse>
                                <demo:Status>Accepted</demo:Status>
                                <demo:HttpStatusCode>200</demo:HttpStatusCode>
                                <demo:RequestMessageId>{SecurityElement.Escape(requestMessageId)}</demo:RequestMessageId>
                                <demo:ResponseMessageId>{responseMessageId}</demo:ResponseMessageId>
                                <demo:DocumentId>{SecurityElement.Escape(documentId)}</demo:DocumentId>
                                <demo:ReceivedAtUtc>{DateTime.UtcNow:O}</demo:ReceivedAtUtc>
                              </demo:SendMessageResponse>
                            </soap:Body>
                          </soap:Envelope>
                          """;

        return new TestEndpointSoapResponse(StatusCodes.Status200OK, "application/soap+xml", responseXml);
    }

    private static TestEndpointSoapResponse CreateAccepted202Response(string requestMessageId, string documentId)
    {
        var responseMessageId = $"{Guid.NewGuid()}@test-endpoint";
        var responseXml = $"""
                          <soap:Envelope xmlns:soap="{SoapNamespace}"
                                         xmlns:demo="{DemoNamespace}">
                            <soap:Body>
                              <demo:SendMessageResponse>
                                <demo:Status>Queued</demo:Status>
                                <demo:HttpStatusCode>202</demo:HttpStatusCode>
                                <demo:RequestMessageId>{SecurityElement.Escape(requestMessageId)}</demo:RequestMessageId>
                                <demo:ResponseMessageId>{responseMessageId}</demo:ResponseMessageId>
                                <demo:DocumentId>{SecurityElement.Escape(documentId)}</demo:DocumentId>
                                <demo:EstimatedProcessingTimeSeconds>30</demo:EstimatedProcessingTimeSeconds>
                                <demo:ReceivedAtUtc>{DateTime.UtcNow:O}</demo:ReceivedAtUtc>
                              </demo:SendMessageResponse>
                            </soap:Body>
                          </soap:Envelope>
                          """;

        return new TestEndpointSoapResponse(StatusCodes.Status202Accepted, "application/soap+xml", responseXml);
    }

    private static TestEndpointSoapResponse CreateBadRequestResponse(string requestMessageId)
    {
        var responseXml = $"""
                          <soap:Envelope xmlns:soap="{SoapNamespace}"
                                         xmlns:demo="{DemoNamespace}">
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
                                    <demo:RequestMessageId>{SecurityElement.Escape(requestMessageId)}</demo:RequestMessageId>
                                  </demo:Error>
                                </soap:Detail>
                              </soap:Fault>
                            </soap:Body>
                          </soap:Envelope>
                          """;

        return new TestEndpointSoapResponse(StatusCodes.Status400BadRequest, "application/soap+xml", responseXml);
    }

    private static TestEndpointSoapResponse CreateNotFoundResponse(string requestMessageId, string documentId)
    {
        var responseXml = $"""
                          <soap:Envelope xmlns:soap="{SoapNamespace}"
                                         xmlns:demo="{DemoNamespace}">
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
                                    <demo:Description>The requested document does not exist.</demo:Description>
                                    <demo:RequestMessageId>{SecurityElement.Escape(requestMessageId)}</demo:RequestMessageId>
                                    <demo:DocumentId>{SecurityElement.Escape(documentId)}</demo:DocumentId>
                                  </demo:Error>
                                </soap:Detail>
                              </soap:Fault>
                            </soap:Body>
                          </soap:Envelope>
                          """;

        return new TestEndpointSoapResponse(StatusCodes.Status404NotFound, "application/soap+xml", responseXml);
    }

    private static string? ExtractValue(string xml, string localName)
    {
        if (string.IsNullOrWhiteSpace(xml))
            return null;

        try
        {
            var normalizedXml = xml.TrimStart().StartsWith("<", StringComparison.Ordinal)
                ? xml
                : $"<Root>{xml}</Root>";

            var document = XDocument.Parse(normalizedXml);

            return document
                .Descendants()
                .FirstOrDefault(element => element.Name.LocalName == localName)
                ?.Value
                ?.Trim();
        }
        catch
        {
            try
            {
                var document = XDocument.Parse($"<Root>{xml}</Root>");

                return document
                    .Descendants()
                    .FirstOrDefault(element => element.Name.LocalName == localName)
                    ?.Value
                    ?.Trim();
            }
            catch
            {
                return null;
            }
        }
    }
}