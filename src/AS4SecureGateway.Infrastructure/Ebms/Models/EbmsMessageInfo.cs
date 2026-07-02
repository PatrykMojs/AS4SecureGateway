using System.Xml.Serialization;
using AS4SecureGateway.Infrastructure.Xml;

namespace AS4SecureGateway.Infrastructure.Ebms.Models;

public sealed class EbmsMessageInfo
{
    [XmlElement("Timestamp", Namespace = XmlNamespaces.Ebms)]
    public string Timestamp { get; set; } = string.Empty;

    [XmlElement("MessageId", Namespace = XmlNamespaces.Ebms)]
    public string MessageId { get; set; } = string.Empty;
}