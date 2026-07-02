using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace AS4SecureGateway.Application.Abstractions.Cryptography;

public interface IXmlBodyEncryptor
{
    void EncryptBody(XmlDocument document, string? bodyContentXml, X509Certificate2 recipientCertificate);
}