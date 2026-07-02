using System.Xml.Serialization;
using AS4SecureGateway.Infrastructure.Xml;

namespace AS4SecureGateway.Infrastructure.Ebms.Models;

public sealed class EbmsPayloadInfo
{
    [XmlElement("PartInfo", Namespace = XmlNamespaces.Ebms)]
    public EbmsPartInfo PartInfo { get; set; } = new();
}