using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace AS4SecureGateway.Application.Abstractions.Cryptography;

public interface IXmlBodySigner
{
    void SignBody(XmlDocument document, string? bodyContentXml, X509Certificate2 signingCertificate, X509Certificate2 publicCertificate);
}