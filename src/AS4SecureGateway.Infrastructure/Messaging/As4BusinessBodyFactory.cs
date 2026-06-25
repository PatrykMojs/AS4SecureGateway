using System.Security;
using AS4SecureGateway.Application.Abstractions.Messaging;
using AS4SecureGateway.Application.Messaging;
using AS4SecureGateway.Application.UseCases.DispatchAs4Message;

namespace AS4SecureGateway.Infrastructure.Messaging;

public sealed class As4BusinessBodyFactory : IAs4BusinessBodyFactory
{
    private const string DemoNamespace = "urn:demo:as4:business-message:v1";

    public string CreateBody(DispatchAs4MessageCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        return command.ActionType switch
        {
            As4ActionType.SendMessage => CreateSendMessageBody(command),
            As4ActionType.PeekMessage => CreatePeekMessageBody(command.PeekMessageDomain),
            As4ActionType.DequeueMessage => CreateDequeueMessageBody(command.DocumentReferenceNumber),
            _ => throw new InvalidOperationException($"Unsupported AS4 action: {command.ActionType}")
        };
    }

    private static string CreateSendMessageBody(DispatchAs4MessageCommand command)
    {
        if(string.IsNullOrWhiteSpace(command.PayloadXml))
            throw new InvalidOperationException("Payload XML is required for SendMessage.");

        return command.PayloadXml;
    }

    private static string CreatePeekMessageBody(string? messageDomain)
    {
        if(string.IsNullOrWhiteSpace(messageDomain))
        {
            return $"""
                    <demo:PeekMessageRequest xmlns:demo="{DemoNamespace}" />
                    """;
        }

        var escapedDomain = SecurityElement.Escape(messageDomain);

        return $"""
                <demo:PeekMessageRequest xmlns:demo="{DemoNamespace}">
                  <demo:MessageDomains>
                    <demo:MessageDomain>{escapedDomain}</demo:MessageDomain>
                  </demo:MessageDomains>
                </demo:PeekMessageRequest>
                """;
    }

    private static string CreateDequeueMessageBody(string? documentReferenceNumber)
    {
        if(string.IsNullOrWhiteSpace(documentReferenceNumber))
            throw new InvalidOperationException("DocumentReferenceNumber is required for DequeueMessage.");

        var escapedReferenceNumber = SecurityElement.Escape(documentReferenceNumber);

         return $"""
                <demo:DequeueMessageRequest xmlns:demo="{DemoNamespace}">
                  <demo:DocumentReferenceNumber>{escapedReferenceNumber}</demo:DocumentReferenceNumber>
                </demo:DequeueMessageRequest>
                """;
    }
}