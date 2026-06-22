using System.Xml;
using System.Xml.Linq;

namespace AS4SecureGateway.Infrastructure.Xml;

public static class XmlFormatter
{
    public static string Format(string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
            throw new ArgumentException("XML content cannot be empty.", nameof(xml));

        var document = XDocument.Parse(xml);

        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            NewLineOnAttributes = false,
            OmitXmlDeclaration = false
        };

        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, settings);

        document.WriteTo(xmlWriter);
        xmlWriter.Flush();

        return stringWriter.ToString();
    }
}