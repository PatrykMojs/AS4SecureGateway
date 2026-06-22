using System.Xml.Serialization;

namespace AS4SecureGateway.Infrastructure.Ebms.Models;

public sealed class EbmsPartyId
{
    [XmlAttribute("type")]
    public string? Type { get; set; }

    [XmlText]
    public string Value { get; set; } = string.Empty;
}