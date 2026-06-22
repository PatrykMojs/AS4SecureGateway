using System.Xml.Serialization;
using AS4SecureGateway.Infrastructure.Xml;

namespace AS4SecureGateway.Infrastructure.Ebms.Models;

public sealed class EbmsParty
{
    [XmlElement("PartyId", Namespace = XmlNamespaces.Ebms)]
    public EbmsPartyId PartyId { get; set; } = new();

    [XmlElement("Role", Namespace = XmlNamespaces.Ebms)]
    public string Role { get; set; } = string.Empty;
}