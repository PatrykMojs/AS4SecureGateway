using System.Security.Cryptography.Xml;
using System.Xml;

namespace AS4SecureGateway.TestEndpoint.Security;

public sealed class TestEndpointSignedXml : SignedXml
{
    public TestEndpointSignedXml(XmlDocument document) : base(document)
    {
    }

    public override XmlElement? GetIdElement(XmlDocument? document, string idValue)
    {
        if (document is null)
            throw new ArgumentNullException(nameof(document));

        var baseElement = base.GetIdElement(document, idValue);

        if (baseElement is not null)
            return baseElement;

        return FindElementById(document.DocumentElement, idValue);
    }

    private static XmlElement? FindElementById(XmlNode? node, string idValue)
    {
        if (node is null)
            return null;

        if (node is XmlElement element && HasMatchingId(element, idValue))
            return element;

        foreach (XmlNode child in node.ChildNodes)
        {
            var found = FindElementById(child, idValue);

            if (found is not null)
                return found;
        }

        return null;
    }

    private static bool HasMatchingId(XmlElement element, string idValue)
    {
        return
            element.GetAttribute("Id") == idValue ||
            element.GetAttribute("ID") == idValue ||
            element.GetAttribute("id") == idValue ||
            element.GetAttribute("Id", TestEndpointXmlNamespaces.WsSecurityUtility) == idValue;
    }
}