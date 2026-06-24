using System.Security.Cryptography.Xml;
using System.Xml;
using AS4SecureGateway.Infrastructure.Xml;

namespace AS4SecureGateway.Infrastructure.Cryptography.Signatures;

public sealed class As4SignedXml : SignedXml
{
    public As4SignedXml(XmlDocument document) : base(document)
    {
    }

    public override XmlElement? GetIdElement(XmlDocument? document, string idValue)
    {
        if(document is null)
            throw new ArgumentNullException(nameof(document));

        if(string.IsNullOrWhiteSpace(idValue))
            throw new ArgumentException("ID value cannot be empty.", nameof(idValue));

        var element = base.GetIdElement(document, idValue);

        if(element is not null)
            return element;

        return FindElementById(document.DocumentElement, idValue);
    }

    private static XmlElement? FindElementById(XmlNode? node, string idValue)
    {
        if(node is null)
            return null;

         if (node is XmlElement element && HasMatchingId(element, idValue))
            return element;

        foreach(XmlNode childNode in node.ChildNodes)
        {
            var found = FindElementById(childNode, idValue);

            if(found is not null)
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
            element.GetAttribute("Id", XmlNamespaces.WsSecurityUtility) == idValue;
    }
}