using System.Net;
using System.Xml;

namespace AS4SecureGateway.Infrastructure.Http;

public sealed record As4HttpCallResult(
    XmlDocument RequestXml,
    HttpStatusCode StatusCode,
    string ResponseBody
)
{
    public bool IsSuccessStatusCode => (int)StatusCode is >= 200 and <= 299;
}