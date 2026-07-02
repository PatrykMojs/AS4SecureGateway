using System.Xml.Serialization;
using AS4SecureGateway.Infrastructure.Xml;

namespace AS4SecureGateway.Infrastructure.Ebms.Models;

public sealed class EbmsMessaging
{
    [XmlAttribute("mustUnderstand", Namespace = XmlNamespaces.SoapEnvelope)]
    public bool MustUnderstand { get; set; } = true;

    [XmlElement("UserMessage", Namespace = XmlNamespaces.Ebms)]
    public EbmsUserMessage UserMessage { get; set; } = new();
}