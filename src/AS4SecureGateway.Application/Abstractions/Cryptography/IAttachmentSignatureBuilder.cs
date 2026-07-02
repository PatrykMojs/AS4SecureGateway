using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace AS4SecureGateway.Application.Abstractions.Cryptography;

public interface IAttachmentSignatureBuilder
{
    void SignCompressedAttachment(
        XmlDocument document, 
        byte[] compressedPayload, 
        X509Certificate2 signingCertificate, 
        X509Certificate2 publicCertificate,
        string contentId);
}