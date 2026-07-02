using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using AS4SecureGateway.TestEndpoint.Options;
using Microsoft.Extensions.Options;

namespace AS4SecureGateway.TestEndpoint.Security;

public sealed class TestEndpointInboundSecurityProcessor
{
    private readonly TestEndpointCertificateOptions _certificateOptions;
    private readonly TestEndpointGZipDecompressor _gzipDecompressor;

    public TestEndpointInboundSecurityProcessor(IOptions<TestEndpointCertificateOptions> certificateOptions, TestEndpointGZipDecompressor gzipDecompressor)
    {
        _certificateOptions = certificateOptions.Value;
        _gzipDecompressor = gzipDecompressor;
    }

    public SecuredInboundAs4Message Process(MultipartAs4Request request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var document = new XmlDocument
        {
            PreserveWhitespace = true
        };

        document.LoadXml(request.EnvelopeXml);

        var payloadContentId = ResolvePayloadContentId(document);

        if (!request.Attachments.TryGetValue(payloadContentId, out var compressedEncryptedBody))
        {
            throw new InvalidOperationException(
                $"MIME attachment '{payloadContentId}' was not found.");
        }

        var encryptedBodyBytes = _gzipDecompressor.Decompress(compressedEncryptedBody);
        var encryptedBodyXml = Encoding.UTF8.GetString(encryptedBodyBytes);

        RestoreEncryptedBody(document, encryptedBodyXml);

        var serverEncryptionCertificate = TestEndpointCertificateLoader.LoadCertificateWithPrivateKey(
            _certificateOptions.Local.PublicEncryptionCertificatePath,
            _certificateOptions.Local.PrivateEncryptionCertificatePath);

        DecryptSoapBody(document, serverEncryptionCertificate);

        var clientSignatureCertificate = TestEndpointCertificateLoader.LoadPublicCertificate(
            _certificateOptions.Remote.PublicSignatureCertificatePath);

        var signatureValid = VerifySoapBodySignature(document, clientSignatureCertificate);

        if (!signatureValid)
            throw new CryptographicException("SOAP Body signature is invalid.");

        var businessBodyXml = GetSoapBodyInnerXml(document);

        return new SecuredInboundAs4Message
        {
            DecryptedEnvelopeXml = document.OuterXml,
            BusinessBodyXml = businessBodyXml,
            PayloadContentId = payloadContentId,
            SignatureValid = signatureValid
        };
    }

    private static string ResolvePayloadContentId(XmlDocument document)
    {
        var ns = TestEndpointXmlNamespaceManagerFactory.Create(document);

        var href = document
            .SelectSingleNode("//eb:PayloadInfo/eb:PartInfo/@href", ns)
            ?.Value;

        if (string.IsNullOrWhiteSpace(href))
            throw new InvalidOperationException("Payload href was not found in eb:PartInfo.");

        return href.StartsWith("cid:", StringComparison.OrdinalIgnoreCase)
            ? href[4..].Trim('<', '>')
            : href.Trim('<', '>');
    }

    private static void RestoreEncryptedBody(XmlDocument document, string encryptedBodyXml)
    {
        if (string.IsNullOrWhiteSpace(encryptedBodyXml))
            throw new InvalidOperationException("Encrypted body XML from attachment is empty.");

        var ns = TestEndpointXmlNamespaceManagerFactory.Create(document);

        var bodyElement = document.SelectSingleNode("//soap:Body", ns) as XmlElement
            ?? throw new InvalidOperationException("SOAP Body element was not found.");

        bodyElement.InnerXml = encryptedBodyXml;
    }

    private static void DecryptSoapBody(XmlDocument document, X509Certificate2 serverEncryptionCertificate)
    {
        var ns = TestEndpointXmlNamespaceManagerFactory.Create(document);

        var encryptedDataElement = document
            .SelectSingleNode("//soap:Body/xenc:EncryptedData", ns) as XmlElement
            ?? throw new InvalidOperationException("xenc:EncryptedData was not found inside SOAP Body.");

        var encryptedKeyElement = document
            .SelectSingleNode("//xenc:EncryptedKey", ns) as XmlElement
            ?? throw new InvalidOperationException("xenc:EncryptedKey was not found.");

        var encryptedData = new EncryptedData();
        encryptedData.LoadXml(encryptedDataElement);

        var encryptedKey = new EncryptedKey();
        encryptedKey.LoadXml(encryptedKeyElement);

        if (encryptedKey.CipherData.CipherValue is null)
            throw new InvalidOperationException("EncryptedKey CipherValue is empty.");

        using var rsa = serverEncryptionCertificate.GetRSAPrivateKey()
            ?? throw new InvalidOperationException("Server encryption certificate does not contain RSA private key.");

        var symmetricKey = rsa.Decrypt(encryptedKey.CipherData.CipherValue, RSAEncryptionPadding.OaepSHA1);

        using var aes = Aes.Create();
        aes.Key = symmetricKey;

        var encryptedXml = new EncryptedXml(document);
        var decryptedData = encryptedXml.DecryptData(encryptedData, aes);

        encryptedXml.ReplaceData(encryptedDataElement, decryptedData);
    }

    private static bool VerifySoapBodySignature(XmlDocument document, X509Certificate2 clientSignatureCertificate)
    {
        var signatureElement = document
            .GetElementsByTagName("Signature", TestEndpointXmlNamespaces.XmlDigitalSignature)
            .OfType<XmlElement>()
            .FirstOrDefault();

        if (signatureElement is null)
            throw new InvalidOperationException("ds:Signature element was not found.");

        var signedXml = new TestEndpointSignedXml(document);

        signedXml.LoadXml(signatureElement);

        return signedXml.CheckSignature(clientSignatureCertificate, verifySignatureOnly: true);
    }

    private static string GetSoapBodyInnerXml(XmlDocument document)
    {
        var ns = TestEndpointXmlNamespaceManagerFactory.Create(document);

        var bodyElement = document.SelectSingleNode("//soap:Body", ns) as XmlElement
            ?? throw new InvalidOperationException("SOAP Body element was not found.");

        return bodyElement.InnerXml;
    }
}