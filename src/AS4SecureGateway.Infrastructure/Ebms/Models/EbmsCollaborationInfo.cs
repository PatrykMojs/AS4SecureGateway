using System.Xml.Serialization;
using AS4SecureGateway.Infrastructure.Xml;

namespace AS4SecureGateway.Infrastructure.Ebms.Models;

public sealed class EbmsCollaborationInfo
{
    [XmlElement("AgreementRef", Namespace = XmlNamespaces.Ebms)]
    public string AgreementRef { get; set; } = string.Empty;

    [XmlElement("Service", Namespace = XmlNamespaces.Ebms)]
    public string Service { get; set; } = string.Empty;

    [XmlElement("Action", Namespace = XmlNamespaces.Ebms)]
    public string Action { get; set; } = string.Empty;

    [XmlElement("ConversationId", Namespace = XmlNamespaces.Ebms)]
    public string ConversationId { get; set; } = string.Empty;
}