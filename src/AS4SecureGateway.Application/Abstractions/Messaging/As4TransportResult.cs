using System.Net;

namespace AS4SecureGateway.Application.Abstractions.Messaging;

public sealed class As4TransportResult
{
    public HttpStatusCode StatusCode { get; init; }
    public string ResponseBody { get; init; } = string.Empty;
}