namespace AS4SecureGateway.Infrastructure.Xml;

public static class XmlNamespaces
{
    public const string SoapEnvelope = "http://www.w3.org/2003/05/soap-envelope";
    public const string Ebms = "http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/200704/";

    public const string WsSecurity = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";
    public const string WsSecurityUtility = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";
    public const string WsSecurity11 = "http://docs.oasis-open.org/wss/oasis-wss-wssecurity-secext-1.1.xsd";

    public const string XmlDigitalSignature = "http://www.w3.org/2000/09/xmldsig#";
    public const string XmlEncryption = "http://www.w3.org/2001/04/xmlenc#";
    public const string ExclusiveCanonicalization = "http://www.w3.org/2001/10/xml-exc-c14n#";

    public const string DemoBusinessMessage = "urn:demo:as4:business-message:v1";
}