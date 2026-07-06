using AS4SecureGateway.Infrastructure.Responses;
using FluentAssertions;

namespace AS4SecureGateway.Infrastructure.Tests.Responses;

public sealed class As4ResponseParserTests
{
    private readonly As4ResponseParser _parser = new();

    [Fact]
    public void Parse_WhenResponseIsEmpty_ShouldReturnEmptyParsedResponse()
    {
        var result = _parser.Parse("   ");

        result.Status.Should().BeNull();
        result.MessageId.Should().BeNull();
        result.DocumentId.Should().BeNull();
        result.ReceivedAtUtc.Should().BeNull();
        result.HasFault.Should().BeFalse();
    }

    [Fact]
    public void Parse_WhenResponseContainsSuccessFields_ShouldParseValuesByLocalName()
    {
        const string responseXml = """
            <soap:Envelope xmlns:soap="http://www.w3.org/2003/05/soap-envelope">
              <soap:Body>
                <demo:SendMessageResponse xmlns:demo="urn:demo:as4:business-message:v1">
                  <demo:Status> Accepted </demo:Status>
                  <demo:MessageId> server-message-id </demo:MessageId>
                  <demo:DocumentId> document-id </demo:DocumentId>
                  <demo:ReceivedAtUtc> 2026-07-04T12:00:00Z </demo:ReceivedAtUtc>
                </demo:SendMessageResponse>
              </soap:Body>
            </soap:Envelope>
            """;

        var result = _parser.Parse(responseXml);

        result.Status.Should().Be("Accepted");
        result.MessageId.Should().Be("server-message-id");
        result.DocumentId.Should().Be("document-id");
        result.ReceivedAtUtc.Should().Be("2026-07-04T12:00:00Z");
        result.HasFault.Should().BeFalse();
    }

    [Fact]
    public void Parse_WhenResponseContainsSoapFault_ShouldParseFaultDetails()
    {
        const string responseXml = """
            <soap:Envelope xmlns:soap="http://www.w3.org/2003/05/soap-envelope">
              <soap:Body>
                <soap:Fault>
                  <soap:Reason>
                    <soap:Text> Invalid AS4 message </soap:Text>
                  </soap:Reason>
                  <soap:Detail>
                    <demo:Error xmlns:demo="urn:demo:as4:business-message:v1">
                      <demo:ErrorCode> AS4-001 </demo:ErrorCode>
                      <demo:Description> Missing required ebMS header </demo:Description>
                    </demo:Error>
                  </soap:Detail>
                </soap:Fault>
              </soap:Body>
            </soap:Envelope>
            """;

        var result = _parser.Parse(responseXml);

        result.HasFault.Should().BeTrue();
        result.FaultReason.Should().Be("Invalid AS4 message");
        result.ErrorCode.Should().Be("AS4-001");
        result.ErrorDescription.Should().Be("Missing required ebMS header");
        result.Status.Should().BeNull();
    }

    [Fact]
    public void Parse_WhenXmlIsInvalid_ShouldThrowXmlException()
    {
        var act = () => _parser.Parse("<broken>");

        act.Should().Throw<System.Xml.XmlException>();
    }
}