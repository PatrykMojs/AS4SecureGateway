using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using AS4SecureGateway.Application.Abstractions.Cryptography;
using AS4SecureGateway.Infrastructure.Cryptography.WsSecurity;
using AS4SecureGateway.Infrastructure.Xml;

namespace AS4SecureGateway.Infrastructure.Cryptography.Encryption;

public sealed class CompressedAttachmentEncryptor : ICompressedAttachmentEncryptor
{
    public EncryptedAttachmentResult EncryptCompressedAttachment(
        XmlDocument document,
        byte[] compressedPayload,
        X509Certificate2 recipientCertificate,
        string contentId)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(compressedPayload);
        ArgumentNullException.ThrowIfNull(recipientCertificate);

        if (compressedPayload.Length == 0)
            throw new ArgumentException("Compressed payload cannot be empty.", nameof(compressedPayload));

        if (string.IsNullOrWhiteSpace(contentId))
            throw new ArgumentException("Content id cannot be empty.", nameof(contentId));

        var binarySecurityTokenId = "X509-" + Guid.NewGuid().ToString("N").ToUpperInvariant();
        var encryptedDataId = "ED-" + Guid.NewGuid().ToString("N").ToUpperInvariant();
        var encryptedKeyId = "EK-" + Guid.NewGuid().ToString("N").ToUpperInvariant();

        var securityElement = WsSecurityHeaderBuilder.GetOrCreateSecurityHeader(document);

        WsSecurityHeaderBuilder.AddBinarySecurityToken(document, securityElement, recipientCertificate.RawData, binarySecurityTokenId);

        using var aes = Aes.Create();
        aes.KeySize = 128;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.GenerateKey();
        aes.GenerateIV();

        using var rsaPublicKey = recipientCertificate.GetRSAPublicKey()
            ?? throw new InvalidOperationException("Recipient certificate does not contain RSA public key.");

        var encryptedKeyBytes = rsaPublicKey.Encrypt(aes.Key, RSAEncryptionPadding.OaepSHA1);

        byte[] encryptedPayloadBytes;

        using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
        {
            encryptedPayloadBytes = encryptor.TransformFinalBlock(compressedPayload, 0, compressedPayload.Length);
        }

        var finalEncryptedPayload = aes.IV.Concat(encryptedPayloadBytes).ToArray();

        var encryptedKeyElement = CreateEncryptedKeyElement(
            document,
            encryptedKeyId,
            encryptedDataId,
            binarySecurityTokenId,
            encryptedKeyBytes);

        var encryptedDataElement = CreateEncryptedDataElement(
            document,
            encryptedDataId,
            encryptedKeyId,
            contentId);

        var namespaceManager = XmlNamespaceManagerFactory.Create(document);

        var signatureElement = securityElement.SelectSingleNode("ds:Signature", namespaceManager);

        if (signatureElement is not null)
        {
            securityElement.InsertBefore(encryptedKeyElement, signatureElement);
            securityElement.InsertBefore(encryptedDataElement, signatureElement);
        }
        else
        {
            securityElement.AppendChild(encryptedKeyElement);
            securityElement.AppendChild(encryptedDataElement);
        }

        return new EncryptedAttachmentResult(
            finalEncryptedPayload,
            encryptedDataId,
            encryptedKeyId,
            binarySecurityTokenId,
            contentId);
    }

    private static XmlElement CreateEncryptedKeyElement(
        XmlDocument document,
        string encryptedKeyId,
        string encryptedDataId,
        string binarySecurityTokenId,
        byte[] encryptedKeyBytes)
    {
        var encryptedKeyElement = document.CreateElement("xenc", "EncryptedKey", XmlNamespaces.XmlEncryption);
        encryptedKeyElement.SetAttribute("Id", encryptedKeyId);

        var encryptionMethodElement = document.CreateElement("xenc", "EncryptionMethod", XmlNamespaces.XmlEncryption);
        encryptionMethodElement.SetAttribute("Algorithm", SecurityAlgorithms.RsaOaepMgf1p);

        var digestMethodElement = document.CreateElement("ds", "DigestMethod", XmlNamespaces.XmlDigitalSignature);
        digestMethodElement.SetAttribute("Algorithm", SignedXmlConstants.Sha1Digest);
        encryptionMethodElement.AppendChild(digestMethodElement);

        encryptedKeyElement.AppendChild(encryptionMethodElement);

        var keyInfoElement = document.CreateElement("ds", "KeyInfo", XmlNamespaces.XmlDigitalSignature);
        var securityTokenReferenceElement = document.CreateElement("wsse", "SecurityTokenReference", XmlNamespaces.WsSecurity);
        var referenceElement = document.CreateElement("wsse", "Reference", XmlNamespaces.WsSecurity);

        referenceElement.SetAttribute("URI", $"#{binarySecurityTokenId}");
        referenceElement.SetAttribute("ValueType", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3");

        securityTokenReferenceElement.AppendChild(referenceElement);
        keyInfoElement.AppendChild(securityTokenReferenceElement);
        encryptedKeyElement.AppendChild(keyInfoElement);

        var cipherDataElement = document.CreateElement("xenc", "CipherData", XmlNamespaces.XmlEncryption);
        var cipherValueElement = document.CreateElement("xenc", "CipherValue", XmlNamespaces.XmlEncryption);
        cipherValueElement.InnerText = Convert.ToBase64String(encryptedKeyBytes);

        cipherDataElement.AppendChild(cipherValueElement);
        encryptedKeyElement.AppendChild(cipherDataElement);

        var referenceListElement = document.CreateElement("xenc", "ReferenceList", XmlNamespaces.XmlEncryption);
        var dataReferenceElement = document.CreateElement("xenc", "DataReference", XmlNamespaces.XmlEncryption);
        dataReferenceElement.SetAttribute("URI", $"#{encryptedDataId}");

        referenceListElement.AppendChild(dataReferenceElement);
        encryptedKeyElement.AppendChild(referenceListElement);

        return encryptedKeyElement;
    }

    private static XmlElement CreateEncryptedDataElement(XmlDocument document, string encryptedDataId, string encryptedKeyId, string contentId)
    {
        var encryptedDataElement = document.CreateElement("xenc", "EncryptedData", XmlNamespaces.XmlEncryption);
        encryptedDataElement.SetAttribute("Id", encryptedDataId);
        encryptedDataElement.SetAttribute("Type", SecurityAlgorithms.AttachmentContentOnly);
        encryptedDataElement.SetAttribute("MimeType", "application/gzip");

        var encryptionMethodElement = document.CreateElement("xenc", "EncryptionMethod", XmlNamespaces.XmlEncryption);
        encryptionMethodElement.SetAttribute("Algorithm", SecurityAlgorithms.Aes128Cbc);
        encryptedDataElement.AppendChild(encryptionMethodElement);

        var keyInfoElement = document.CreateElement("ds", "KeyInfo", XmlNamespaces.XmlDigitalSignature);
        var securityTokenReferenceElement = document.CreateElement("wsse", "SecurityTokenReference", XmlNamespaces.WsSecurity);

        var tokenTypeAttribute = document.CreateAttribute("wsse11", "TokenType", XmlNamespaces.WsSecurity11);
        tokenTypeAttribute.Value = "http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1#EncryptedKey";
        securityTokenReferenceElement.Attributes.Append(tokenTypeAttribute);

        var referenceElement = document.CreateElement("wsse", "Reference", XmlNamespaces.WsSecurity);
        referenceElement.SetAttribute("URI", $"#{encryptedKeyId}");

        securityTokenReferenceElement.AppendChild(referenceElement);
        keyInfoElement.AppendChild(securityTokenReferenceElement);
        encryptedDataElement.AppendChild(keyInfoElement);

        var cipherDataElement = document.CreateElement("xenc", "CipherData", XmlNamespaces.XmlEncryption);
        var cipherReferenceElement = document.CreateElement("xenc", "CipherReference", XmlNamespaces.XmlEncryption);
        cipherReferenceElement.SetAttribute("URI", $"cid:{contentId}");

        var transformsElement = document.CreateElement("xenc", "Transforms", XmlNamespaces.XmlEncryption);
        var transformElement = document.CreateElement("ds", "Transform", XmlNamespaces.XmlDigitalSignature);
        transformElement.SetAttribute("Algorithm", SecurityAlgorithms.AttachmentCiphertextTransform);

        transformsElement.AppendChild(transformElement);
        cipherReferenceElement.AppendChild(transformsElement);
        cipherDataElement.AppendChild(cipherReferenceElement);
        encryptedDataElement.AppendChild(cipherDataElement);

        return encryptedDataElement;
    }

    private static class SignedXmlConstants
    {
        public const string Sha1Digest = "http://www.w3.org/2000/09/xmldsig#sha1";
    }
}