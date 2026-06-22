using System.Xml.Serialization;
using AS4SecureGateway.Infrastructure.Xml;

namespace AS4SecureGateway.Infrastructure.Soap.Models;

[XmlRoot("Envelope", Namespace = XmlNamespaces.SoapEnvelope)]
public sealed class SoapEnvelope
{
    [XmlElement("Header", Namespace = XmlNamespaces.SoapEnvelope)]
    public SoapHeader Header { get; set; } = new();

    [XmlElement("Body", Namespace = XmlNamespaces.SoapEnvelope)]
    public SoapBody Body { get; set; } = new();
}