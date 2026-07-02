using System.Xml.Serialization;

namespace AS4SecureGateway.Infrastructure.Ebms.Models;

public sealed class EbmsProperty
{
    [XmlAttribute("name")]
    public string Name { get; set; } = string.Empty;

    [XmlText]
    public string Value { get; set; } = string.Empty;
}