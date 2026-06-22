using System.Xml.Serialization;
using AS4SecureGateway.Infrastructure.Xml;

namespace AS4SecureGateway.Infrastructure.Ebms.Models;

public sealed class EbmsPartyInfo
{
    [XmlElement("From", Namespace = XmlNamespaces.Ebms)]
    public EbmsParty From { get; set; } = new();

    [XmlElement("To", Namespace = XmlNamespaces.Ebms)]
    public EbmsParty To { get; set; } = new();
}