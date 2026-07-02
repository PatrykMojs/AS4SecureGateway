using System.Xml;
using AS4SecureGateway.Infrastructure.Xml;

namespace AS4SecureGateway.Infrastructure.Cryptography.WsSecurity;

public static class WsSecurityHeaderBuilder
{
    public static XmlElement GetOrCreateSecurityHeader(XmlDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var namespaceManager = XmlNamespaceManagerFactory.Create(document);

        var headerElement = document.SelectSingleNode("//soap:Header", namespaceManager) as XmlElement
            ?? throw new InvalidOperationException("Could not find SOAP Header element.");

        var securityElement = document.SelectSingleNode("//soap:Header/wsse:Security", namespaceManager) as XmlElement;

        if (securityElement is not null)
            return securityElement;

        securityElement = document.CreateElement("wsse", "Security", XmlNamespaces.WsSecurity);


        var mustUnderstandAttribute = document.CreateAttribute("soap", "mustUnderstand", XmlNamespaces.SoapEnvelope);
        mustUnderstandAttribute.Value = "1";
        securityElement.Attributes.Append(mustUnderstandAttribute);

        headerElement.AppendChild(securityElement);

        return securityElement;
    }

    public static XmlElement AddBinarySecurityToken(XmlDocument document, XmlElement securityElement, byte[] certificateRawData, string tokenId)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(securityElement);
        ArgumentNullException.ThrowIfNull(certificateRawData);

        var tokenElement = document.CreateElement("wsse", "BinarySecurityToken", XmlNamespaces.WsSecurity);

        tokenElement.SetAttribute("EncodingType", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary");
        tokenElement.SetAttribute("ValueType", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3");

        var idAttribute = document.CreateAttribute("wsu", "Id", XmlNamespaces.WsSecurityUtility);
        idAttribute.Value = tokenId;
        tokenElement.Attributes.Append(idAttribute);

        tokenElement.InnerText = Convert.ToBase64String(certificateRawData);

        securityElement.InsertBefore(tokenElement, securityElement.FirstChild);

        return tokenElement;
    }

    public static XmlElement CreateSecurityTokenReference(XmlDocument document, string binarySecurityTokenId, string? securityTokenReferenceId = null)
    {
        ArgumentNullException.ThrowIfNull(document);

        var securityTokenReferenceElement = document.CreateElement("wsse", "SecurityTokenReference", XmlNamespaces.WsSecurity);

        if (!string.IsNullOrWhiteSpace(securityTokenReferenceId))
        {
            var idAttribute = document.CreateAttribute("wsu", "Id", XmlNamespaces.WsSecurityUtility);
            idAttribute.Value = securityTokenReferenceId;
            securityTokenReferenceElement.Attributes.Append(idAttribute);
        }

        var referenceElement = document.CreateElement("wsse", "Reference", XmlNamespaces.WsSecurity);
        referenceElement.SetAttribute("URI", "#" + binarySecurityTokenId);
        referenceElement.SetAttribute("ValueType", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3");

        securityTokenReferenceElement.AppendChild(referenceElement);

        return securityTokenReferenceElement;
    }
}