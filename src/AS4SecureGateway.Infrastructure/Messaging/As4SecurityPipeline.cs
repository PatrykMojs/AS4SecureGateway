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
                certificates.SigningCertificate,
                certificates.SigningPublicCertificate);
        }

        if (securityOptions.EnableEncryption)
        {
            _xmlBodyEncryptor.EncryptBody(
                envelope,
                securityOptions.EnableSignature ? null : businessBodyXml,
                certificates.RecipientEncryptionCertificate);
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
        var payloadBytes = Encoding.UTF8.GetBytes(businessBodyXml);
        var compressedPayload = _payloadCompressor.Compress(payloadBytes);

        byte[] attachmentContent = compressedPayload;

        if (certificates is null)
            throw new InvalidOperationException("Certificates are required for signing or encryption.");

        if (securityOptions.EnableSignature)
        {
            _attachmentSignatureBuilder.SignCompressedAttachment(
                envelope,
                compressedPayload,
                certificates.SigningCertificate,
                certificates.SigningPublicCertificate,
                As4MessageDefaults.PayloadContentId);
        }

        if (securityOptions.EnableEncryption)
        {
            var encryptedAttachment = _compressedAttachmentEncryptor.EncryptCompressedAttachment(
                envelope,
                compressedPayload,
                certificates.RecipientEncryptionCertificate,
                As4MessageDefaults.PayloadContentId);

            attachmentContent = encryptedAttachment.EncryptedPayload;
        }

        var attachments = new Dictionary<string, byte[]>
        {
            [As4MessageDefaults.PayloadContentId] = attachmentContent
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
}