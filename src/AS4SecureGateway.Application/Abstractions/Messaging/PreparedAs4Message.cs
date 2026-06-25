using System.Xml;

namespace AS4SecureGateway.Application.Abstractions.Messaging;

public sealed class PreparedAs4Message
{
    public XmlDocument Envelope { get; init; } = new();
    public string ContentType { get; init; } = "application/soap+xml";
    public IReadOnlyDictionary<string, byte[]> Attachments { get; init; } = new Dictionary<string, byte[]>();
}