using System.Xml.Serialization;
using AS4SecureGateway.Infrastructure.Xml;

namespace AS4SecureGateway.Infrastructure.Ebms.Models;

public sealed class EbmsUserMessage
{
    [XmlElement("MessageInfo", Namespace = XmlNamespaces.Ebms)]
    public EbmsMessageInfo MessageInfo { get; set; } = new();

    [XmlElement("PartyInfo", Namespace = XmlNamespaces.Ebms)]
    public EbmsPartyInfo PartyInfo { get; set; } = new();

    [XmlElement("CollaborationInfo", Namespace = XmlNamespaces.Ebms)]
    public EbmsCollaborationInfo CollaborationInfo { get; set; } = new();

    [XmlElement("PayloadInfo", Namespace = XmlNamespaces.Ebms)]
    public EbmsPayloadInfo PayloadInfo { get; set; } = new();
}