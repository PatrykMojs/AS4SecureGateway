using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using AS4SecureGateway.Application.Abstractions.Cryptography;
using AS4SecureGateway.Infrastructure.Cryptography.WsSecurity;
using AS4SecureGateway.Infrastructure.Xml;

namespace AS4SecureGateway.Infrastructure.Cryptography.Signatures;

public sealed class AttachmentSignatureBuilder : IAttachmentSignatureBuilder
{
    public void SignCompressedAttachment(
        XmlDocument document,
        byte[] compressedPayload,
        X509Certificate2 signingCertificate,
        X509Certificate2 publicCertificate,
        string contentId)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(compressedPayload);
        ArgumentNullException.ThrowIfNull(signingCertificate);
        ArgumentNullException.ThrowIfNull(publicCertificate);

        if (compressedPayload.Length == 0)
            throw new ArgumentException("Compressed payload cannot be empty.", nameof(compressedPayload));

        if (string.IsNullOrWhiteSpace(contentId))
            throw new ArgumentException("Content id cannot be empty.", nameof(contentId));

        var binarySecurityTokenId = "X509-" + Guid.NewGuid().ToString("N").ToUpperInvariant();
        var securityElement = WsSecurityHeaderBuilder.GetOrCreateSecurityHeader(document);

        WsSecurityHeaderBuilder.AddBinarySecurityToken(document, securityElement, publicCertificate.RawData, binarySecurityTokenId);

        var securityTokenReferenceElement = WsSecurityHeaderBuilder.CreateSecurityTokenReference(document, binarySecurityTokenId);
        var attachmentDigest = SHA256.HashData(compressedPayload);

        var referenceElement = CreateReferenceElement(
            document,
            $"cid:{contentId}",
            SecurityAlgorithms.AttachmentContentSignatureTransform,
            attachmentDigest,
            SignedXml.XmlDsigSHA256Url);

        var signedInfoElement = CreateSignedInfoElement(document, referenceElement);
        var canonicalizedSignedInfo = Canonicalize(signedInfoElement);

        using var privateKey = signingCertificate.GetRSAPrivateKey()
            ?? throw new InvalidOperationException("RSA private key is missing in signing certificate.");

        var signatureValue = privateKey.SignData(canonicalizedSignedInfo, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        
        var signatureElement = document.CreateElement("ds", "Signature", XmlNamespaces.XmlDigitalSignature);
        signatureElement.AppendChild(document.ImportNode(signedInfoElement, deep: true));

        var signatureValueElement = document.CreateElement("ds", "SignatureValue", XmlNamespaces.XmlDigitalSignature);
        signatureValueElement.InnerText = Convert.ToBase64String(signatureValue);
        signatureElement.AppendChild(signatureValueElement);

        var keyInfoElement = document.CreateElement("ds", "KeyInfo", XmlNamespaces.XmlDigitalSignature);
        keyInfoElement.AppendChild(document.ImportNode(securityTokenReferenceElement, deep: true));
        signatureElement.AppendChild(keyInfoElement);

        securityElement.AppendChild(signatureElement);
    }

    private static XmlElement CreateReferenceElement(
        XmlDocument ownerDocument,
        string referenceUri,
        string transformAlgorithm,
        byte[] digestValue,
        string digestMethod)
    {
        var referenceElement = ownerDocument.CreateElement("ds", "Reference", XmlNamespaces.XmlDigitalSignature);
        referenceElement.SetAttribute("URI", referenceUri);

        var transformsElement = ownerDocument.CreateElement("ds", "Transforms", XmlNamespaces.XmlDigitalSignature);

        var transformElement = ownerDocument.CreateElement("ds", "Transform", XmlNamespaces.XmlDigitalSignature);
        transformElement.SetAttribute("Algorithm", transformAlgorithm);

        transformsElement.AppendChild(transformElement);
        referenceElement.AppendChild(transformsElement);

        var digestMethodElement = ownerDocument.CreateElement("ds", "DigestMethod", XmlNamespaces.XmlDigitalSignature);
        digestMethodElement.SetAttribute("Algorithm", digestMethod);
        referenceElement.AppendChild(digestMethodElement);

        var digestValueElement = ownerDocument.CreateElement("ds", "DigestValue", XmlNamespaces.XmlDigitalSignature);
        digestValueElement.InnerText = Convert.ToBase64String(digestValue);
        referenceElement.AppendChild(digestValueElement);

        return referenceElement;
    }

    private static XmlElement CreateSignedInfoElement(XmlDocument ownerDocument, XmlElement referenceElement)
    {
        var signedInfoElement = ownerDocument.CreateElement("ds", "SignedInfo", XmlNamespaces.XmlDigitalSignature);

        var canonicalizationMethodElement = ownerDocument.CreateElement("ds", "CanonicalizationMethod", XmlNamespaces.XmlDigitalSignature);
        canonicalizationMethodElement.SetAttribute("Algorithm", SignedXml.XmlDsigExcC14NTransformUrl);
        signedInfoElement.AppendChild(canonicalizationMethodElement);

        var signatureMethodElement = ownerDocument.CreateElement("ds", "SignatureMethod", XmlNamespaces.XmlDigitalSignature);
        signatureMethodElement.SetAttribute("Algorithm", SecurityAlgorithms.RsaSha256);
        signedInfoElement.AppendChild(signatureMethodElement);

        signedInfoElement.AppendChild(ownerDocument.ImportNode(referenceElement, deep: true));

        return signedInfoElement;
    }

    private static byte[] Canonicalize(XmlElement element)
    {
        var tempDocument = new XmlDocument
        {
            PreserveWhitespace = true
        };

        var importedElement = tempDocument.ImportNode(element, deep: true);
        tempDocument.AppendChild(importedElement);

        var transform = new XmlDsigExcC14NTransform();
        transform.LoadInput(tempDocument);

        var outputStream = transform.GetOutput(typeof(Stream)) as Stream
            ?? throw new InvalidOperationException("Canonicalization output is not a stream.");

        using var memoryStream = new MemoryStream();
        outputStream.CopyTo(memoryStream);

        return memoryStream.ToArray();
    }
}