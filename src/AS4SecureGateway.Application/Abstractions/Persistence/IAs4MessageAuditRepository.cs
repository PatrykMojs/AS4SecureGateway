namespace AS4SecureGateway.Application.Abstractions.Persistence;

public interface IAs4MessageAuditRepository
{
    Task SaveAsync(As4MessageAuditRecord record, CancellationToken cancellationToken);
}