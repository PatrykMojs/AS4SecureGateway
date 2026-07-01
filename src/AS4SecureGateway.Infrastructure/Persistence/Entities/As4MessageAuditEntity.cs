namespace AS4SecureGateway.Infrastructure.Persistence.Entities;

public sealed class As4MessageAuditEntity
{
    public Guid Id { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public int HttpStatusCode { get; set; }
    public bool IsSuccess { get; set; }
    public string RequestXml { get; set; } = string.Empty;
    public string ResponseBody { get; set; } = string.Empty;
    public string? Status { get; set; }
    public string? MessageId { get; set; }
    public string? DocumentId { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorDescription { get; set; }
    public string? FaultReason { get; set; }
}