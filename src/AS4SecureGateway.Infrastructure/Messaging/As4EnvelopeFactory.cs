using System.Text;
using System.Xml;
using System.Xml.Serialization;
using AS4SecureGateway.Application.Abstractions.Messaging;
using AS4SecureGateway.Application.Messaging;
using AS4SecureGateway.Infrastructure.Ebms.Models;
using AS4SecureGateway.Infrastructure.Soap.Models;
using AS4SecureGateway.Infrastructure.Xml;

namespace AS4SecureGateway.Infrastructure.Messaging;

public sealed class As4EnvelopeFactory : IAs4EnvelopeFactory
{
    public XmlDocument CreateEnvelope(As4MessageMetadata metaData, string? payloadHref)
    {
        ArgumentNullException.ThrowIfNull(metaData);

        var envelope = new SoapEnvelope
        {
            Header = new SoapHeader
            {
                Messaging = new EbmsMessaging
                {
                    MustUnderstand = true,
                    UserMessage = new EbmsUserMessage
                    {
                        MessageInfo = new EbmsMessageInfo
                        {
                            Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                            MessageId = CreateMessageId()
                        },
                        PartyInfo = new EbmsPartyInfo
                        {
                            From = new EbmsParty
                            {
                                PartyId = new EbmsPartyId
                                {
                                    Value = metaData.SenderPartyId
                                },
                                Role = metaData.SenderRole
                            },
                            To = new EbmsParty
                            {
                                PartyId = new EbmsPartyId
                                {
                                    Value = metaData.ReceiverPartyId
                                },
                                Role = metaData.ReceiverPartyId
                            }
                        },
                        CollaborationInfo = new EbmsCollaborationInfo
                        {
                            AgreementRef = metaData.AgreementRef,
                            Service = metaData.Service,
                            Action = metaData.Action,
                            ConversationId = metaData.ConversationId
                        },
                        PayloadInfo = new EbmsPayloadInfo
                        {
                            PartInfo = CreatePartInfo(metaData, payloadHref)
                        }
                    }
                }
            },
            Body = new SoapBody()
        };

        var xml = SerializeEnvelope(envelope);

        var document = new XmlDocument
        {
            PreserveWhitespace = true
        };

        document.LoadXml(xml);

        return document;
    }

    private static EbmsPartInfo CreatePartInfo(As4MessageMetadata metadata, string? payloadHref)
    {
        var partInfo = new EbmsPartInfo
        {
            Href = payloadHref ?? string.Empty
        };

        var properties = new List<EbmsProperty>();

        if (!string.IsNullOrWhiteSpace(metadata.MimeType))
        {
            properties.Add(new EbmsProperty
            {
                Name = "MimeType",
                Value = metadata.MimeType
            });
        }

        if (!string.IsNullOrWhiteSpace(metadata.CharacterSet))
        {
            properties.Add(new EbmsProperty
            {
                Name = "CharacterSet",
                Value = metadata.CharacterSet
            });
        }

        if (!string.IsNullOrWhiteSpace(metadata.CompressionType))
        {
            properties.Add(new EbmsProperty
            {
                Name = "CompressionType",
                Value = metadata.CompressionType
            });
        }

        if (properties.Count > 0)
        {
            partInfo.PartProperties = new EbmsPartProperties
            {
                Properties = properties
            };
        }

        return partInfo;
    }

    private static string SerializeEnvelope(SoapEnvelope envelope)
    {
        var namespaces = new XmlSerializerNamespaces();

        namespaces.Add("soap", XmlNamespaces.SoapEnvelope);
        namespaces.Add("eb", XmlNamespaces.Ebms);
        namespaces.Add("wsu", XmlNamespaces.WsSecurityUtility);
        namespaces.Add("wsse", XmlNamespaces.WsSecurity);
        namespaces.Add("xenc", XmlNamespaces.XmlEncryption);
        namespaces.Add("ds", XmlNamespaces.XmlDigitalSignature);

        var serializer = new XmlSerializer(typeof(SoapEnvelope));

        var settings = new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            Indent = false,
            OmitXmlDeclaration = true
        };

        using var stringWriter = new Utf8StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, settings);

        serializer.Serialize(xmlWriter, envelope, namespaces);

        return stringWriter.ToString();
    }

    private static string CreateMessageId()
    {
        return $"{Guid.NewGuid():N}@as4-secure-gateway";
    }

    private sealed class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}