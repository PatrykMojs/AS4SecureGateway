using System.Xml;
using AS4SecureGateway.Application.Messaging;

namespace AS4SecureGateway.Application.Abstractions.Messaging;

public interface IAs4EnvelopeFactory
{
    XmlDocument CreateEnvelope(As4MessageMetadata metaData, string? payloadHref);
}