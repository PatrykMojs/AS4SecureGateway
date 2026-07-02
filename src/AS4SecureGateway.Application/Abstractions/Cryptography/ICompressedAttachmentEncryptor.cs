using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace AS4SecureGateway.Application.Abstractions.Cryptography;

public interface ICompressedAttachmentEncryptor
{
    EncryptedAttachmentResult EncryptCompressedAttachment(XmlDocument document, byte[] compressedPayload, X509Certificate2 recipientCertificate, string contentId);
}