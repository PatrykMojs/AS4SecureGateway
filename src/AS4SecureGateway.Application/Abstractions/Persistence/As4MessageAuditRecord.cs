using AS4SecureGateway.Application.Messaging;

namespace AS4SecureGateway.Application.Abstractions.Persistence;

public sealed class As4MessageAuditRecord
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public As4ActionType ActionType { get; init; }
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
    public int HttpStatusCode { get; init; }
    public bool IsSuccess { get; init; }
    public string RequestXml { get; init; } = string.Empty;
    public string ResponseBody { get; init; } = string.Empty;
    public string? Status { get; init; }
    public string? MessageId { get; init; }
    public string? DocumentId { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorDescription { get; init; }
    public string? FaultReason { get; init; }
}