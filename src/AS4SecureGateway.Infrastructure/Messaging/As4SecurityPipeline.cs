using System.Text;
using System.Xml;
using AS4SecureGateway.Application.Abstractions.Certificates;
using AS4SecureGateway.Application.Abstractions.Compression;
using AS4SecureGateway.Application.Abstractions.Cryptography;
using AS4SecureGateway.Application.Abstractions.Messaging;
using AS4SecureGateway.Application.Messaging;
using AS4SecureGateway.Infrastructure.Xml;

namespace AS4SecureGateway.Infrastructure.Messaging;

public sealed class As4SecurityPipeline : IAs4SecurityPipeline
{
    private readonly IPayloadCompressor _payloadCompressor;
    private readonly IXmlBodySigner _xmlBodySigner;
    private readonly IXmlBodyEncryptor _xmlBodyEncryptor;
    private readonly IAttachmentSignatureBuilder _attachmentSignatureBuilder;
    private readonly ICompressedAttachmentEncryptor _compressedAttachmentEncryptor;
    private readonly IAs4CertificateProvider _certificateProvider;

    public As4SecurityPipeline(
        IPayloadCompressor payloadCompressor,
        IXmlBodySigner xmlBodySigner,
        IXmlBodyEncryptor xmlBodyEncryptor,
        IAttachmentSignatureBuilder attachmentSignatureBuilder,
        ICompressedAttachmentEncryptor compressedAttachmentEncryptor,
        IAs4CertificateProvider certificateProvider)
    {
        _payloadCompressor = payloadCompressor;
        _xmlBodySigner = xmlBodySigner;
        _xmlBodyEncryptor = xmlBodyEncryptor;
        _attachmentSignatureBuilder = attachmentSignatureBuilder;
        _compressedAttachmentEncryptor = compressedAttachmentEncryptor;
        _certificateProvider = certificateProvider;
    }

    public async Task<PreparedAs4Message> PrepareAsync(
        XmlDocument envelope, 
        string businessBodyXml, 
        SecurityProcessingOptions securityOptions, 
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        ArgumentNullException.ThrowIfNull(securityOptions);

        if(string.IsNullOrWhiteSpace(businessBodyXml))
            throw new ArgumentException("Business body XML cannot be empty.", nameof(businessBodyXml));

        var requiresCertificates = 
            securityOptions.EnableEncryption ||
            securityOptions.EnableSignature;

        var certificates = requiresCertificates
            ? await _certificateProvider.GetCertificatesAsync(cancellationToken)
            : null;

        if (securityOptions.EnableCompression)
        {
            return PrepareCompressedMessage(
                envelope,
                businessBodyXml,
                securityOptions,
                certificates);
        }

        return PrepareSoapBodyMessage(
            envelope,
            businessBodyXml,
            securityOptions,
            certificates);
    }

    private PreparedAs4Message PrepareSoapBodyMessage(
        XmlDocument envelope,
        string businessBodyXml,
        SecurityProcessingOptions securityOptions,
        As4CertificateSet? certificates)
    {
        if (!securityOptions.EnableSignature && !securityOptions.EnableEncryption)
        {
            InsertBusinessBody(envelope, businessBodyXml);

            return new PreparedAs4Message
            {
                Envelope = envelope,
                ContentType = As4MessageDefaults.SoapContentType,
                Attachments = new Dictionary<string, byte[]>()
            };
        }

        if (certificates is null)
            throw new InvalidOperationException("Certificates are required for signing or encryption.");

        if (securityOptions.EnableSignature)
        {
            _xmlBodySigner.SignBody(
                envelope,
                businessBodyXml,
                certificates.LocalPrivateSignatureCertificate,
                certificates.LocalPublicSignatureCertificate);
        }

        if (securityOptions.EnableEncryption)
        {
            _xmlBodyEncryptor.EncryptBody(
                envelope,
                securityOptions.EnableSignature ? null : businessBodyXml,
                certificates.RemotePublicEncryptionCertificate);
        }

        return new PreparedAs4Message
        {
            Envelope = envelope,
            ContentType = As4MessageDefaults.SoapContentType,
            Attachments = new Dictionary<string, byte[]>()
        };
    }

    private PreparedAs4Message PrepareCompressedMessage(
        XmlDocument envelope,
        string businessBodyXml,
        SecurityProcessingOptions securityOptions,
        As4CertificateSet? certificates)
    {
        if (certificates is null)
            throw new InvalidOperationException("Certificates are required for secured compressed body attachment.");

        if (!securityOptions.EnableSignature || !securityOptions.EnableEncryption)
        {
            throw new InvalidOperationException(
                "Compressed external body attachment requires both signature and encryption to be enabled.");
        }

        _xmlBodySigner.SignBody(
            envelope,
            businessBodyXml,
            certificates.LocalPrivateSignatureCertificate,
            certificates.LocalPublicSignatureCertificate);

        _xmlBodyEncryptor.EncryptBody(
            envelope,
            bodyContentXml: null,
            certificates.RemotePublicEncryptionCertificate);

        var bodyElement = GetSoapBody(envelope);

        var encryptedBodyXml = bodyElement.InnerXml;

        if (string.IsNullOrWhiteSpace(encryptedBodyXml))
            throw new InvalidOperationException("Encrypted SOAP Body content is empty.");

        var encryptedBodyBytes = Encoding.UTF8.GetBytes(encryptedBodyXml);

        var compressedEncryptedBody = _payloadCompressor.Compress(encryptedBodyBytes);

        RemoveBodyChildren(bodyElement);

        var attachments = new Dictionary<string, byte[]>
        {
            [As4MessageDefaults.PayloadContentId] = compressedEncryptedBody
        };

        return new PreparedAs4Message
        {
            Envelope = envelope,
            ContentType = As4MessageDefaults.MultipartContentType,
            Attachments = attachments
        };
    }

    private static void InsertBusinessBody(XmlDocument envelope, string businessBodyXml)
    {
        var namespaceManager = XmlNamespaceManagerFactory.Create(envelope);

        var bodyElement = envelope.SelectSingleNode("//soap:Body", namespaceManager) as XmlElement
            ?? throw new InvalidOperationException("SOAP Body element was not found.");

        bodyElement.InnerXml = businessBodyXml;
    }

    private static XmlElement GetSoapBody(XmlDocument envelope)
    {
        var namespaceManager = XmlNamespaceManagerFactory.Create(envelope);

        return envelope.SelectSingleNode("//soap:Body", namespaceManager) as XmlElement
            ?? throw new InvalidOperationException("SOAP Body element was not found.");
    }

    private static void RemoveBodyChildren(XmlElement bodyElement)
    {
        while (bodyElement.FirstChild is not null)
        {
            bodyElement.RemoveChild(bodyElement.FirstChild);
        }
    }
}