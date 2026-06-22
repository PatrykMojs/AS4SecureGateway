using System.Xml.Serialization;
using AS4SecureGateway.Infrastructure.Xml;

namespace AS4SecureGateway.Infrastructure.Ebms.Models;

public sealed class EbmsPartInfo
{
    [XmlAttribute("href")]
    public string Href { get; set; } = string.Empty;

    [XmlElement("PartProperties", Namespace = XmlNamespaces.Ebms)]
    public EbmsPartProperties? PartProperties { get; set; }
}