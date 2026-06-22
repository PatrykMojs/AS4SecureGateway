using System.Xml;

namespace AS4SecureGateway.Infrastructure.Xml;

public static class XmlNamespaceManagerFactory
{
    public static XmlNamespaceManager Create(XmlDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var namespaceManager = new XmlNamespaceManager(document.NameTable);

        namespaceManager.AddNamespace("soap", XmlNamespaces.SoapEnvelope);
        namespaceManager.AddNamespace("env", XmlNamespaces.SoapEnvelope);
        namespaceManager.AddNamespace("soapenv", XmlNamespaces.SoapEnvelope);

        namespaceManager.AddNamespace("eb", XmlNamespaces.Ebms);

        namespaceManager.AddNamespace("wsse", XmlNamespaces.WsSecurity);
        namespaceManager.AddNamespace("wsu", XmlNamespaces.WsSecurityUtility);
        namespaceManager.AddNamespace("wsse11", XmlNamespaces.WsSecurity11);

        namespaceManager.AddNamespace("ds", XmlNamespaces.XmlDigitalSignature);
        namespaceManager.AddNamespace("dsig", XmlNamespaces.XmlDigitalSignature);

        namespaceManager.AddNamespace("xenc", XmlNamespaces.XmlEncryption);
        namespaceManager.AddNamespace("ec", XmlNamespaces.ExclusiveCanonicalization);

        namespaceManager.AddNamespace("demo", XmlNamespaces.DemoBusinessMessage);

        return namespaceManager;
    }
}