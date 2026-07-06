using System.Xml.Linq;
using AS4SecureGateway.Application.Abstractions.Responses;

namespace AS4SecureGateway.Infrastructure.Responses;

public sealed class As4ResponseParser : IAs4ResponseParser
{
    public As4ParsedResponse Parse(string responseXml)
    {
        if(string.IsNullOrWhiteSpace(responseXml))
            return As4ParsedResponse.Empty;

        var document = XDocument.Parse(responseXml);

        var faultElement = document
            .Descendants()
            .FirstOrDefault(element => element.Name.LocalName == "Fault");

        if(faultElement is not null)
            return ParseFault(faultElement);

        return ParseSuccessResponse(document);
    }

    private static As4ParsedResponse ParseSuccessResponse(XDocument document)
    {
        return new As4ParsedResponse
        {
            Status = GetValue(document, "Status"),
            MessageId = GetValue(document, "MessageId") ?? GetValue(document, "ResponseMessageId"),
            DocumentId = GetValue(document, "DocumentId"),
            ReceivedAtUtc = GetValue(document, "ReceivedAtUtc")
        };
    }

    private static As4ParsedResponse ParseFault(XElement faultElement)
    {
        return new As4ParsedResponse
        {
            FaultReason = faultElement
                .Descendants()
                .FirstOrDefault(element => element.Name.LocalName == "Text")
                ?.Value
                ?.Trim(),

            ErrorCode = faultElement
                .Descendants()
                .FirstOrDefault(element => element.Name.LocalName == "ErrorCode")
                ?.Value
                ?.Trim(),

                ErrorDescription = faultElement
                    .Descendants()
                    .FirstOrDefault(element => element.Name.LocalName == "Description")
                    ?.Value
                    ?.Trim()
        };
    }

    private static string? GetValue(XDocument document, string localName)
    {
        return document
            .Descendants()
            .FirstOrDefault(element => element.Name.LocalName == localName)
            ?.Value
            ?.Trim();
    }
}