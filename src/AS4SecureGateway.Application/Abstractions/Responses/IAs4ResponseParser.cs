namespace AS4SecureGateway.Application.Abstractions.Responses;

public interface IAs4ResponseParser
{
    As4ParsedResponse Parse(string responseXml);
}