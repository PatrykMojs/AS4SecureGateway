using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using AS4SecureGateway.Application.Abstractions.Cryptography;
using AS4SecureGateway.Infrastructure.Cryptography.WsSecurity;
using AS4SecureGateway.Infrastructure.Xml;

namespace AS4SecureGateway.Infrastructure.Cryptography.Encryption;

public sealed class XmlBodyEncryptor : IXmlBodyEncryptor
{
    public void EncryptBody(XmlDocument document, string? bodyContentXml, X509Certificate2 recipientCertificate)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(recipientCertificate);

        var namespaceManager = XmlNamespaceManagerFactory.Create(document);

        var bodyElement = GetOrCreateBody(document, namespaceManager);

        if (!string.IsNullOrWhiteSpace(bodyContentXml))
            bodyElement.InnerXml = bodyContentXml;

        var payloadElement = bodyElement.FirstChild as XmlElement
            ?? throw new InvalidOperationException("SOAP Body does not contain an XML payload to encrypt.");

        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.GenerateKey();
        aes.GenerateIV();

        var encryptedXml = new EncryptedXml();
        var encryptedElementBytes = encryptedXml.EncryptData(payloadElement, aes, content: false);

        var encryptedData = new EncryptedData
        {
            Id = "ED-" + Guid.NewGuid().ToString("N").ToUpperInvariant(),
            Type = EncryptedXml.XmlEncElementContentUrl,
            EncryptionMethod = new EncryptionMethod(EncryptedXml.XmlEncAES256Url)
        };

        encryptedData.CipherData.CipherValue = encryptedElementBytes;

        using var rsaPublicKey = recipientCertificate.GetRSAPublicKey()
            ?? throw new InvalidOperationException("Recipient certificate does not contain RSA public key.");

        var encryptedKey = new EncryptedKey
        {
            Id = "EK-" + Guid.NewGuid().ToString("N").ToUpperInvariant(),
            EncryptionMethod = new EncryptionMethod(EncryptedXml.XmlEncRSAOAEPUrl),
            CipherData = new CipherData(EncryptedXml.EncryptKey(aes.Key, rsaPublicKey, useOAEP: true))
        };

        encryptedKey.AddReference(new DataReference($"#{encryptedData.Id}"));

        var keyReferenceForBody = CreateEncryptedKeyReference(document, encryptedKey.Id);
        var keyInfoForBody = new KeyInfo();
        keyInfoForBody.AddClause(new KeyInfoNode(keyReferenceForBody));
        encryptedData.KeyInfo = keyInfoForBody;

        ReplaceBodyContentWithEncryptedData(document, bodyElement, encryptedData);

        var securityElement = WsSecurityHeaderBuilder.GetOrCreateSecurityHeader(document);

        var keyReferenceForHeader = CreateCertificateKeyInfo(document, recipientCertificate);
        var keyInfoForHeader = new KeyInfo();
        keyInfoForHeader.AddClause(new KeyInfoNode(keyReferenceForHeader));
        encryptedKey.KeyInfo = keyInfoForHeader;

        var encryptedKeyElement = document.ImportNode(encryptedKey.GetXml(), deep: true) as XmlElement
            ?? throw new InvalidOperationException("Could not import EncryptedKey XML element.");

        encryptedKeyElement.Prefix = "xenc";

        if (securityElement.FirstChild is not null)
            securityElement.InsertBefore(encryptedKeyElement, securityElement.FirstChild);
        else
            securityElement.AppendChild(encryptedKeyElement);

        NormalizeEncryptionPrefixes(document);
    }

    private static XmlElement GetOrCreateBody(XmlDocument document, XmlNamespaceManager namespaceManager)
    {
        var bodyElement = document.SelectSingleNode("//soap:Body", namespaceManager) as XmlElement;

        if (bodyElement is not null)
            return bodyElement;

        var envelopeElement = document.DocumentElement
            ?? throw new InvalidOperationException("SOAP Envelope element was not found.");

        bodyElement = document.CreateElement("soap", "Body", XmlNamespaces.SoapEnvelope);
        envelopeElement.AppendChild(bodyElement);

        return bodyElement;
    }

    private static XmlElement CreateEncryptedKeyReference(XmlDocument document, string encryptedKeyId)
    {
        var securityTokenReference = document.CreateElement("wsse", "SecurityTokenReference", XmlNamespaces.WsSecurity);

        var tokenTypeAttribute = document.CreateAttribute("wsse11", "TokenType", XmlNamespaces.WsSecurity11);
        tokenTypeAttribute.Value = "http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1#EncryptedKey";
        securityTokenReference.Attributes.Append(tokenTypeAttribute);

        var referenceElement = document.CreateElement("wsse", "Reference", XmlNamespaces.WsSecurity);
        referenceElement.SetAttribute("URI", $"#{encryptedKeyId}");

        securityTokenReference.AppendChild(referenceElement);

        return securityTokenReference;
    }

    private static XmlElement CreateCertificateKeyInfo(XmlDocument document, X509Certificate2 certificate)
    {
        var securityTokenReference = document.CreateElement("wsse", "SecurityTokenReference", XmlNamespaces.WsSecurity);

        var keyIdentifierElement = document.CreateElement("wsse", "KeyIdentifier", XmlNamespaces.WsSecurity);
        keyIdentifierElement.SetAttribute("EncodingType", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary");
        keyIdentifierElement.SetAttribute("ValueType", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3");

        keyIdentifierElement.InnerText = Convert.ToBase64String(certificate.GetRawCertData());

        securityTokenReference.AppendChild(keyIdentifierElement);

        return securityTokenReference;
    }

    private static void ReplaceBodyContentWithEncryptedData(XmlDocument document, XmlElement bodyElement, EncryptedData encryptedData)
    {
        var attributes = bodyElement.Attributes
            .Cast<XmlAttribute>()
            .Select(attribute => attribute.CloneNode(deep: true))
            .Cast<XmlAttribute>()
            .ToList();

        bodyElement.RemoveAll();

        foreach (var attribute in attributes)
        {
            bodyElement.Attributes.Append(attribute);
        }

        var encryptedDataElement = document.ImportNode(encryptedData.GetXml(), deep: true) as XmlElement
            ?? throw new InvalidOperationException("Could not import EncryptedData XML element.");

        encryptedDataElement.Prefix = "xenc";
        bodyElement.AppendChild(encryptedDataElement);
    }

    private static void NormalizeEncryptionPrefixes(XmlDocument document)
    {
        SetPrefix(document, "EncryptedData", "xenc");
        SetPrefix(document, "EncryptedKey", "xenc");
        SetPrefix(document, "EncryptionMethod", "xenc");
        SetPrefix(document, "CipherData", "xenc");
        SetPrefix(document, "CipherValue", "xenc");
        SetPrefix(document, "ReferenceList", "xenc");
        SetPrefix(document, "DataReference", "xenc");
        SetPrefix(document, "KeyInfo", "ds");
    }

    private static void SetPrefix(XmlDocument document, string localName, string prefix)
    {
        var nodes = document.GetElementsByTagName(localName);

        foreach (XmlNode node in nodes)
        {
            if (node is XmlElement element)
                element.Prefix = prefix;
        }
    }
}