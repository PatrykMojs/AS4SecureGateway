using System.Xml.Serialization;
using AS4SecureGateway.Infrastructure.Xml;

namespace AS4SecureGateway.Infrastructure.Ebms.Models;

public sealed class EbmsPartProperties
{
    [XmlElement("Property", Namespace = XmlNamespaces.Ebms)]
    public List<EbmsProperty> Properties { get; set; } = new();
}