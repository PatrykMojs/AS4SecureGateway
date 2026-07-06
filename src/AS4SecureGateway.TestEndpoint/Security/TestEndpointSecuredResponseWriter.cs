using System.IO.Compression;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using AS4SecureGateway.TestEndpoint.Options;
using AS4SecureGateway.TestEndpoint.Responses;
using Microsoft.Extensions.Options;

namespace AS4SecureGateway.TestEndpoint.Security;

public sealed class TestEndpointSecuredResponseWriter
{
    private const string SoapRootContentId = "root.response@localhost";
    private const string PayloadContentId = "ebms3-response-payload@localhost";
    private const string PayloadHref = "cid:" + PayloadContentId;

    private readonly TestEndpointCertificateOptions _certificateOptions;

    public TestEndpointSecuredResponseWriter(
        IOptions<TestEndpointCertificateOptions> certificateOptions)
    {
        _certificateOptions = certificateOptions.Value;
    }

    public async Task WriteAsync(
        HttpResponse httpResponse,
        TestEndpointSoapResponse plainResponse,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpResponse);
        ArgumentNullException.ThrowIfNull(plainResponse);

        var document = new XmlDocument
        {
            PreserveWhitespace = true
        };

        document.LoadXml(plainResponse.Body);

        EnsureResponseEnvelopeHeader(document);
        EnsurePayloadInfo(document);
        EnsureBodyId(document);

        var serverSignatureCertificate = TestEndpointCertificateLoader.LoadCertificateWithPrivateKey(
            _certificateOptions.Local.PublicSignatureCertificatePath,
            _certificateOptions.Local.PrivateSignatureCertificatePath);

        var clientEncryptionCertificate = TestEndpointCertificateLoader.LoadPublicCertificate(
            _certificateOptions.Remote.PublicEncryptionCertificatePath);

        SignSoapBody(document, serverSignatureCertificate);
        EncryptSoapBody(document, clientEncryptionCertificate);

        var bodyElement = GetSoapBody(document);
        var encryptedBodyXml = bodyElement.InnerXml;

        if (string.IsNullOrWhiteSpace(encryptedBodyXml))
            throw new InvalidOperationException("Encrypted response Body is empty.");

        var compressedEncryptedBody = Compress(
            Encoding.UTF8.GetBytes(encryptedBodyXml));

        RemoveBodyChildren(bodyElement);

        var boundary = "----as4-response-boundary-" + Guid.NewGuid().ToString("N");

        var responseBytes = BuildMultipartResponse(
            boundary,
            document.OuterXml,
            compressedEncryptedBody);

        var contentType = $"multipart/related; type=\"application/soap+xml\"; start=\"<{SoapRootContentId}>\"; boundary=\"{boundary}\"";

        logger.LogInformation(
            """
            ================= SECURED AS4 RESPONSE CREATED =================

            HTTP Status: {StatusCode}
            Content-Type: {ContentType}
            SOAP Root Content-ID: {SoapRootContentId}
            Payload Content-ID: {PayloadContentId}
            Response bytes: {ResponseLength}

            ----------------- RESPONSE ENVELOPE WITH EMPTY BODY -----------------

            {EnvelopeXml}

            ===================================================================
            """,
            plainResponse.StatusCode,
            contentType,
            SoapRootContentId,
            PayloadContentId,
            responseBytes.Length,
            document.OuterXml);

        httpResponse.StatusCode = plainResponse.StatusCode;
        httpResponse.ContentType = contentType;

        await httpResponse.Body.WriteAsync(responseBytes, cancellationToken);
    }

    private static void EnsureResponseEnvelopeHeader(XmlDocument document)
    {
        var envelope = document.DocumentElement
            ?? throw new InvalidOperationException("SOAP Envelope was not found.");

        EnsureNamespace(envelope, "eb", TestEndpointXmlNamespaces.Ebms);
        EnsureNamespace(envelope, "wsse", TestEndpointXmlNamespaces.WsSecurity);
        EnsureNamespace(envelope, "wsu", TestEndpointXmlNamespaces.WsSecurityUtility);
        EnsureNamespace(envelope, "ds", TestEndpointXmlNamespaces.XmlDigitalSignature);
        EnsureNamespace(envelope, "xenc", TestEndpointXmlNamespaces.XmlEncryption);

        var ns = TestEndpointXmlNamespaceManagerFactory.Create(document);

        var header = document.SelectSingleNode("//soap:Header", ns) as XmlElement;

        if (header is not null)
            return;

        header = document.CreateElement("soap", "Header", TestEndpointXmlNamespaces.SoapEnvelope);

        var body = document.SelectSingleNode("//soap:Body", ns)
            ?? throw new InvalidOperationException("SOAP Body was not found.");

        envelope.InsertBefore(header, body);
    }

    private static void EnsurePayloadInfo(XmlDocument document)
    {
        var ns = TestEndpointXmlNamespaceManagerFactory.Create(document);

        var header = document.SelectSingleNode("//soap:Header", ns) as XmlElement
            ?? throw new InvalidOperationException("SOAP Header was not found.");

        var existingMessaging = document.SelectSingleNode("//eb:Messaging", ns);

        if (existingMessaging is not null)
            header.RemoveChild(existingMessaging);

        var messaging = document.CreateElement("eb", "Messaging", TestEndpointXmlNamespaces.Ebms);
        SetSoapMustUnderstand(document, messaging, "true");

        var userMessage = document.CreateElement("eb", "UserMessage", TestEndpointXmlNamespaces.Ebms);

        var messageInfo = document.CreateElement("eb", "MessageInfo", TestEndpointXmlNamespaces.Ebms);
        AppendTextElement(document, messageInfo, "eb", "Timestamp", TestEndpointXmlNamespaces.Ebms, DateTime.UtcNow.ToString("O"));
        AppendTextElement(document, messageInfo, "eb", "MessageId", TestEndpointXmlNamespaces.Ebms, $"{Guid.NewGuid():N}@as4-test-endpoint");

        var collaborationInfo = document.CreateElement("eb", "CollaborationInfo", TestEndpointXmlNamespaces.Ebms);
        AppendTextElement(document, collaborationInfo, "eb", "AgreementRef", TestEndpointXmlNamespaces.Ebms, "urn:demo:as4:agreement:Response");
        AppendTextElement(document, collaborationInfo, "eb", "Service", TestEndpointXmlNamespaces.Ebms, "DemoMarketMessaging");
        AppendTextElement(document, collaborationInfo, "eb", "Action", TestEndpointXmlNamespaces.Ebms, "SendMessage.Response");
        AppendTextElement(document, collaborationInfo, "eb", "ConversationId", TestEndpointXmlNamespaces.Ebms, $"{DateTime.UtcNow:yyyy}-{Guid.NewGuid():N}");

        var payloadInfo = document.CreateElement("eb", "PayloadInfo", TestEndpointXmlNamespaces.Ebms);

        var partInfo = document.CreateElement("eb", "PartInfo", TestEndpointXmlNamespaces.Ebms);
        partInfo.SetAttribute("href", PayloadHref);

        payloadInfo.AppendChild(partInfo);

        userMessage.AppendChild(messageInfo);
        userMessage.AppendChild(collaborationInfo);
        userMessage.AppendChild(payloadInfo);

        messaging.AppendChild(userMessage);
        header.AppendChild(messaging);
    }

    private static void SignSoapBody(XmlDocument document, System.Security.Cryptography.X509Certificates.X509Certificate2 signingCertificate)
    {
        var security = EnsureSecurityHeader(document);
        var body = GetSoapBody(document);

        var bodyId = body.GetAttribute("Id", TestEndpointXmlNamespaces.WsSecurityUtility);

        if (string.IsNullOrWhiteSpace(bodyId))
            throw new InvalidOperationException("SOAP Body wsu:Id was not found.");

        using var rsa = signingCertificate.GetRSAPrivateKey()
            ?? throw new InvalidOperationException("Server signature certificate does not contain RSA private key.");

        var signedXml = new TestEndpointSignedXml(document)
        {
            SigningKey = rsa
        };

        signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl;
        signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA256Url;

        var reference = new Reference("#" + bodyId);
        reference.AddTransform(new XmlDsigExcC14NTransform());
        reference.DigestMethod = SignedXml.XmlDsigSHA256Url;

        signedXml.AddReference(reference);

        var keyInfo = new KeyInfo();
        keyInfo.AddClause(new KeyInfoX509Data(signingCertificate));
        signedXml.KeyInfo = keyInfo;

        signedXml.ComputeSignature();

        var signatureElement = signedXml.GetXml();

        security.AppendChild(
            document.ImportNode(signatureElement, deep: true));
    }

    private static void EncryptSoapBody(XmlDocument document, System.Security.Cryptography.X509Certificates.X509Certificate2 recipientCertificate)
    {
        var body = GetSoapBody(document);

        using var rsa = recipientCertificate.GetRSAPublicKey()
            ?? throw new InvalidOperationException("Client encryption certificate does not contain RSA public key.");

        using var aes = Aes.Create();
        aes.KeySize = 256;

        var encryptedXml = new EncryptedXml(document);

        var encryptedBodyBytes = encryptedXml.EncryptData(body, aes, content: true);

        var encryptedData = new EncryptedData
        {
            Type = EncryptedXml.XmlEncElementContentUrl,
            EncryptionMethod = new EncryptionMethod(EncryptedXml.XmlEncAES256Url),
            CipherData = new CipherData(encryptedBodyBytes)
        };

        var encryptedKeyBytes = EncryptedXml.EncryptKey(aes.Key, rsa, useOAEP: true);

        var encryptedKey = new EncryptedKey
        {
            EncryptionMethod = new EncryptionMethod(EncryptedXml.XmlEncRSAOAEPUrl),
            CipherData = new CipherData(encryptedKeyBytes)
        };

        encryptedData.KeyInfo.AddClause(new KeyInfoEncryptedKey(encryptedKey));

        var encryptedDataElement = encryptedData.GetXml();

        RemoveBodyChildren(body);

        body.AppendChild(document.ImportNode(encryptedDataElement, deep: true));
    }

    private static XmlElement EnsureSecurityHeader(XmlDocument document)
    {
        var ns = TestEndpointXmlNamespaceManagerFactory.Create(document);

        var header = document.SelectSingleNode("//soap:Header", ns) as XmlElement
            ?? throw new InvalidOperationException("SOAP Header was not found.");

        var security = document.SelectSingleNode("//wsse:Security", ns) as XmlElement;

        if (security is not null)
            return security;

        security = document.CreateElement("wsse", "Security", TestEndpointXmlNamespaces.WsSecurity);
        SetSoapMustUnderstand(document, security, "1");

        header.AppendChild(security);

        return security;
    }

    private static void EnsureBodyId(XmlDocument document)
    {
        var body = GetSoapBody(document);

        var existingId = body.GetAttribute("Id", TestEndpointXmlNamespaces.WsSecurityUtility);

        if (!string.IsNullOrWhiteSpace(existingId))
            return;

        var idAttribute = document.CreateAttribute("wsu", "Id", TestEndpointXmlNamespaces.WsSecurityUtility);

        idAttribute.Value = "body-" + Guid.NewGuid().ToString("N");

        body.Attributes.Append(idAttribute);
    }

    private static XmlElement GetSoapBody(XmlDocument document)
    {
        var ns = TestEndpointXmlNamespaceManagerFactory.Create(document);

        return document.SelectSingleNode("//soap:Body", ns) as XmlElement
            ?? throw new InvalidOperationException("SOAP Body was not found.");
    }

    private static void RemoveBodyChildren(XmlElement body)
    {
        while (body.FirstChild is not null)
        {
            body.RemoveChild(body.FirstChild);
        }
    }

    private static byte[] Compress(byte[] data)
    {
        using var output = new MemoryStream();

        using (var gzip = new GZipStream(output, CompressionMode.Compress, leaveOpen: true))
        {
            gzip.Write(data, 0, data.Length);
        }

        return output.ToArray();
    }

    private static byte[] BuildMultipartResponse(string boundary, string envelopeXml, byte[] attachmentBytes)
    {
        using var stream = new MemoryStream();

        WriteAscii(stream, $"--{boundary}\r\n");
        WriteAscii(stream, "Content-Type: application/soap+xml; charset=utf-8\r\n");
        WriteAscii(stream, $"Content-ID: <{SoapRootContentId}>\r\n");
        WriteAscii(stream, "Content-Transfer-Encoding: 8bit\r\n");
        WriteAscii(stream, "\r\n");
        WriteUtf8(stream, envelopeXml);
        WriteAscii(stream, "\r\n");

        WriteAscii(stream, $"--{boundary}\r\n");
        WriteAscii(stream, "Content-Type: application/octet-stream\r\n");
        WriteAscii(stream, $"Content-ID: <{PayloadContentId}>\r\n");
        WriteAscii(stream, "Content-Transfer-Encoding: binary\r\n");
        WriteAscii(stream, "\r\n");

        stream.Write(attachmentBytes, 0, attachmentBytes.Length);

        WriteAscii(stream, "\r\n");
        WriteAscii(stream, $"--{boundary}--\r\n");

        return stream.ToArray();
    }

    private static void WriteAscii(Stream stream, string value)
    {
        var bytes = Encoding.ASCII.GetBytes(value);
        stream.Write(bytes, 0, bytes.Length);
    }

    private static void WriteUtf8(Stream stream, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        stream.Write(bytes, 0, bytes.Length);
    }

    private static void AppendTextElement(
        XmlDocument document,
        XmlElement parent,
        string prefix,
        string localName,
        string namespaceUri,
        string value)
    {
        var element = document.CreateElement(prefix, localName, namespaceUri);
        element.InnerText = value;

        parent.AppendChild(element);
    }

    private static void EnsureNamespace(XmlElement element, string prefix, string namespaceUri)
    {
        if (element.HasAttribute("xmlns:" + prefix))
            return;

        var document = element.OwnerDocument
            ?? throw new InvalidOperationException("Owner document was not found.");

        var attribute = document.CreateAttribute("xmlns", prefix, "http://www.w3.org/2000/xmlns/");
        attribute.Value = namespaceUri;

        element.Attributes.Append(attribute);
    }

    private static void SetSoapMustUnderstand(XmlDocument document, XmlElement element, string value)
    {
        var attribute = document.CreateAttribute("soap", "mustUnderstand", TestEndpointXmlNamespaces.SoapEnvelope);

        attribute.Value = value;
        element.Attributes.Append(attribute);
    }
}