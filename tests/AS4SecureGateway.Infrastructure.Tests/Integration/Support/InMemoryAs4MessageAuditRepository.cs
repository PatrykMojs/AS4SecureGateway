using AS4SecureGateway.Application.Abstractions.Persistence;

namespace AS4SecureGateway.Infrastructure.Tests.Integration.Support;

internal sealed class InMemoryAs4MessageAuditRepository : IAs4MessageAuditRepository
{
    private readonly List<As4MessageAuditRecord> _records = new();
    public IReadOnlyList<As4MessageAuditRecord> Records => _records;

    public Task SaveAsync(As4MessageAuditRecord record, CancellationToken cancellationToken)
    {
        _records.Add(record);
        return Task.CompletedTask;
    }
}
