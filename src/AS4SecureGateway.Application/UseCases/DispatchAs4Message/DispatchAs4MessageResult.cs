using System.Net;
using AS4SecureGateway.Application.Abstractions.Responses;
using AS4SecureGateway.Application.Messaging;

namespace AS4SecureGateway.Application.UseCases.DispatchAs4Message;

public sealed class DispatchAs4MessageResult
{
    public As4ActionType ActionType { get; init; }
    public string RequestXml { get; init; } = string.Empty;
    public HttpStatusCode StatusCode { get; init; }
    public string ResponseBody { get; init; } = string.Empty;
    public string? DecompressedResponseBody { get; init; }
    public As4ParsedResponse ParsedResponse { get; init; } = As4ParsedResponse.Empty; 
    public bool IsSuccessStatusCode => (int)StatusCode is >= 200 and <= 299;
}