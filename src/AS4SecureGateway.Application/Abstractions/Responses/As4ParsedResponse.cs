namespace AS4SecureGateway.Application.Abstractions.Responses;

public sealed class As4ParsedResponse
{
    public string? Status { get; init; }
    public string? MessageId { get; init; }
    public string? DocumentId { get; init; }
    public string? ReceivedAtUtc { get; init; }

    public string? ErrorCode { get; init; }
    public string? ErrorDescription { get; init; }
    public string? FaultReason { get; init; }

    public bool HasFault =>
        !string.IsNullOrWhiteSpace(ErrorCode) ||
        !string.IsNullOrWhiteSpace(ErrorDescription) ||
        !string.IsNullOrWhiteSpace(FaultReason);

    public static As4ParsedResponse Empty => new();
}