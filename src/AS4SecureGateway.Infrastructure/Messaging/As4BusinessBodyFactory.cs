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
            _ => throw new InvalidOperationException($"Unsupported AS4 action: {command.ActionType}")
        };
    }

    private static string CreateSendMessageBody(DispatchAs4MessageCommand command)
    {
        if(string.IsNullOrWhiteSpace(command.PayloadXml))
            throw new InvalidOperationException("Payload XML is required for SendMessage.");

        var testScenario = SecurityElement.Escape(command.TestScenario.ToString()) ?? "Ok";

        return $"""
                <demo:SendMessageRequest xmlns:demo="{DemoNamespace}">
                  <demo:MessageContainer>
                    <demo:Payload>
                      {command.PayloadXml}
                    </demo:Payload>
                  </demo:MessageContainer>
                  <demo:TestScenario>{testScenario}</demo:TestScenario>
                </demo:SendMessageRequest>
                """;
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
}