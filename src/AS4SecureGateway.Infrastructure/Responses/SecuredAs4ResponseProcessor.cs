using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using AS4SecureGateway.Application.Abstractions.Compression;
using AS4SecureGateway.Infrastructure.Certificates;
using AS4SecureGateway.Infrastructure.Cryptography.Signatures;
using AS4SecureGateway.Infrastructure.Xml;
using Microsoft.Extensions.Options;

namespace AS4SecureGateway.Infrastructure.Responses;

public sealed class SecuredAs4ResponseProcessor
{
    private readonly IPayloadDecompressor _payloadDecompressor;
    private readonly As4CertificateOptions _certificateOptions;

    public SecuredAs4ResponseProcessor(IPayloadDecompressor payloadDecompressor, IOptions<As4CertificateOptions> certificateOptions)
    {
        _payloadDecompressor = payloadDecompressor;
        _certificateOptions = certificateOptions.Value;
    }

    public string Process(byte[] responseBytes, string contentType)
    {
        var multipartResponse = MultipartAs4ResponseReader.Read(responseBytes, contentType);

        var document = new XmlDocument
        {
            PreserveWhitespace = true
        };

        document.LoadXml(multipartResponse.EnvelopeXml);

        var payloadContentId = ResolvePayloadContentId(document);

        if (!multipartResponse.Attachments.TryGetValue(payloadContentId, out var compressedEncryptedBody))
        {
            throw new InvalidOperationException(
                $"MIME response attachment '{payloadContentId}' was not found.");
        }

        var encryptedBodyBytes = _payloadDecompressor.Decompress(compressedEncryptedBody);
        var encryptedBodyXml = Encoding.UTF8.GetString(encryptedBodyBytes);

        RestoreEncryptedBody(document, encryptedBodyXml);

        var clientEncryptionCertificate = PemCertificateLoader.LoadCertificateWithPrivateKey(
            _certificateOptions.Local.PublicEncryptionCertificatePath,
            _certificateOptions.Local.PrivateEncryptionCertificatePath);

        DecryptSoapBody(document, clientEncryptionCertificate);

        var serverSignatureCertificate = PemCertificateLoader.LoadPublicCertificate(_certificateOptions.Remote.PublicSignatureCertificatePath);

        var signatureValid = VerifySoapBodySignature(document, serverSignatureCertificate);

        if (!signatureValid)
            throw new CryptographicException("AS4 response SOAP Body signature is invalid.");

        return document.OuterXml;
    }

    private static string ResolvePayloadContentId(XmlDocument document)
    {
        var ns = XmlNamespaceManagerFactory.Create(document);

        var href = document
            .SelectSingleNode("//eb:PayloadInfo/eb:PartInfo/@href", ns)
            ?.Value;

        if (string.IsNullOrWhiteSpace(href))
            throw new InvalidOperationException("Response payload href was not found in eb:PartInfo.");

        return href.StartsWith("cid:", StringComparison.OrdinalIgnoreCase)
            ? href[4..].Trim('<', '>')
            : href.Trim('<', '>');
    }

    private static void RestoreEncryptedBody(XmlDocument document, string encryptedBodyXml)
    {
        if (string.IsNullOrWhiteSpace(encryptedBodyXml))
            throw new InvalidOperationException("Encrypted response body XML from attachment is empty.");

        var ns = XmlNamespaceManagerFactory.Create(document);

        var bodyElement = document.SelectSingleNode("//soap:Body", ns) as XmlElement
            ?? throw new InvalidOperationException("SOAP Body element was not found in response.");

        bodyElement.InnerXml = encryptedBodyXml;
    }

    private static void DecryptSoapBody(XmlDocument document, X509Certificate2 clientEncryptionCertificate)
    {
        var ns = XmlNamespaceManagerFactory.Create(document);

        var encryptedDataElement = document
            .SelectSingleNode("//soap:Body/xenc:EncryptedData", ns) as XmlElement
            ?? throw new InvalidOperationException("xenc:EncryptedData was not found inside response SOAP Body.");

        var encryptedKeyElement = document
            .SelectSingleNode("//xenc:EncryptedKey", ns) as XmlElement
            ?? throw new InvalidOperationException("xenc:EncryptedKey was not found in response.");

        var encryptedData = new EncryptedData();
        encryptedData.LoadXml(encryptedDataElement);

        var encryptedKey = new EncryptedKey();
        encryptedKey.LoadXml(encryptedKeyElement);

        if (encryptedKey.CipherData.CipherValue is null)
            throw new InvalidOperationException("Response EncryptedKey CipherValue is empty.");

        using var rsa = clientEncryptionCertificate.GetRSAPrivateKey()
            ?? throw new InvalidOperationException("Client encryption certificate does not contain RSA private key.");

        var symmetricKey = rsa.Decrypt(encryptedKey.CipherData.CipherValue, RSAEncryptionPadding.OaepSHA1);

        using var aes = Aes.Create();
        aes.Key = symmetricKey;

        var encryptedXml = new EncryptedXml(document);
        var decryptedData = encryptedXml.DecryptData(encryptedData, aes);

        encryptedXml.ReplaceData(encryptedDataElement, decryptedData);
    }

    private static bool VerifySoapBodySignature(XmlDocument document, X509Certificate2 serverSignatureCertificate)
    {
        var signatureElement = document
            .GetElementsByTagName("Signature", XmlNamespaces.XmlDigitalSignature)
            .OfType<XmlElement>()
            .FirstOrDefault();

        if (signatureElement is null)
            throw new InvalidOperationException("ds:Signature element was not found in response.");

        var signedXml = new As4SignedXml(document);

        signedXml.LoadXml(signatureElement);

        return signedXml.CheckSignature(serverSignatureCertificate, verifySignatureOnly: true);
    }
}