using System.Xml.Serialization;
using AS4SecureGateway.Infrastructure.Ebms.Models;
using AS4SecureGateway.Infrastructure.Xml;

namespace AS4SecureGateway.Infrastructure.Soap.Models;

public sealed class SoapHeader
{
    [XmlElement("Messaging", Namespace = XmlNamespaces.Ebms)]
    public EbmsMessaging Messaging { get; set; } = new();
}