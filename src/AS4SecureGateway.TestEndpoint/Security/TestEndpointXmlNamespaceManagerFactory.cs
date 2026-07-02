using System.Xml;

namespace AS4SecureGateway.TestEndpoint.Security;

public static class TestEndpointXmlNamespaceManagerFactory
{
    public static XmlNamespaceManager Create(XmlDocument document)
    {
        var namespaceManager = new XmlNamespaceManager(document.NameTable);

        namespaceManager.AddNamespace("soap", TestEndpointXmlNamespaces.SoapEnvelope);
        namespaceManager.AddNamespace("eb", TestEndpointXmlNamespaces.Ebms);
        namespaceManager.AddNamespace("wsse", TestEndpointXmlNamespaces.WsSecurity);
        namespaceManager.AddNamespace("wsu", TestEndpointXmlNamespaces.WsSecurityUtility);
        namespaceManager.AddNamespace("ds", TestEndpointXmlNamespaces.XmlDigitalSignature);
        namespaceManager.AddNamespace("xenc", TestEndpointXmlNamespaces.XmlEncryption);

        return namespaceManager;
    }
}