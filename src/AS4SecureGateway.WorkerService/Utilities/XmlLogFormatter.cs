using System.Xml.Linq;

namespace AS4SecureGateway.WorkerService.Utilities;

public static class XmlLogFormatter
{
    public static string Format(string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
            return string.Empty;

        try
        {
            var document = XDocument.Parse(xml);

            return document.ToString();
        }
        catch
        {
            return xml;
        }
    }

    public static string FormatFragment(string xmlFragment)
    {
        if (string.IsNullOrWhiteSpace(xmlFragment))
            return string.Empty;

        try
        {
            var document = XDocument.Parse($"<Root>{xmlFragment}</Root>");

            return document.ToString();
        }
        catch
        {
            return xmlFragment;
        }
    }
}