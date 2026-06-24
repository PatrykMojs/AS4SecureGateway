using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using AS4SecureGateway.Application.Abstractions.Cryptography;
using AS4SecureGateway.Infrastructure.Cryptography.WsSecurity;
using AS4SecureGateway.Infrastructure.Xml;

namespace AS4SecureGateway.Infrastructure.Cryptography.Signatures;

public sealed class XmlBodySigner : IXmlBodySigner
{
    public void SignBody(XmlDocument document, string? bodyContentXml, X509Certificate2 signingCertificate, X509Certificate2 publicCertificate)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(signingCertificate);
        ArgumentNullException.ThrowIfNull(publicCertificate);

        var namespaceManager = XmlNamespaceManagerFactory.Create(document);

        var bodyElement = GetOrCreateBody(document, namespaceManager);

        if (!string.IsNullOrWhiteSpace(bodyContentXml))
            bodyElement.InnerXml = bodyContentXml;

        var bodyId = "id-" + Guid.NewGuid().ToString("N").ToUpperInvariant();
        var binarySecurityTokenId = "X509-" + Guid.NewGuid().ToString("N").ToUpperInvariant();
        var signatureId = "SIG-" + Guid.NewGuid().ToString("N").ToUpperInvariant();
        var keyInfoId = "KI-" + Guid.NewGuid().ToString("N").ToUpperInvariant();
        var securityTokenReferenceId = "STR-" + Guid.NewGuid().ToString("N").ToUpperInvariant();

        SetWsuId(document, bodyElement, bodyId);

        var securityElement = WsSecurityHeaderBuilder.GetOrCreateSecurityHeader(document);

        WsSecurityHeaderBuilder.AddBinarySecurityToken(document, securityElement, publicCertificate.RawData, binarySecurityTokenId);

        var securityTokenReferenceElement = WsSecurityHeaderBuilder.CreateSecurityTokenReference(
            document,
            binarySecurityTokenId,
            securityTokenReferenceId);

        using var privateKey = signingCertificate.GetRSAPrivateKey()
            ?? throw new InvalidOperationException("RSA private key is missing in signing certificate.");

        var signedXml = new As4SignedXml(document)
        {
            SigningKey = privateKey
        };

        var transform = new XmlDsigExcC14NTransform
        {
            InclusiveNamespacesPrefixList = "eb wsse wsu"
        };

        var reference = new Reference("#" + bodyId)
        {
            DigestMethod = SignedXml.XmlDsigSHA256Url
        };

        reference.AddTransform(transform);
        signedXml.AddReference(reference);

        if (signedXml.SignedInfo is null)
            throw new InvalidOperationException("SignedInfo could not be initialized.");

        signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl;
        signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA256Url;

        var keyInfo = new KeyInfo
        {
            Id = keyInfoId
        };

        keyInfo.AddClause(new KeyInfoNode(securityTokenReferenceElement));
        signedXml.KeyInfo = keyInfo;

        signedXml.ComputeSignature();

        var signatureElement = signedXml.GetXml();

        var signatureIdAttribute = document.CreateAttribute("Id");
        signatureIdAttribute.Value = signatureId;
        signatureElement.Attributes.Append(signatureIdAttribute);

        securityElement.AppendChild(document.ImportNode(signatureElement, deep: true));
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

    private static void SetWsuId(XmlDocument document, XmlElement element, string id)
    {
        var idAttribute = document.CreateAttribute("wsu", "Id", XmlNamespaces.WsSecurityUtility);
        idAttribute.Value = id;
        element.Attributes.Append(idAttribute);
    }
}