using AS4SecureGateway.Application.Abstractions.Persistence;
using AS4SecureGateway.Infrastructure.Persistence.Entities;

namespace AS4SecureGateway.Infrastructure.Persistence.Repositories;

public sealed class SqliteAs4MessageAuditRepository : IAs4MessageAuditRepository
{
    private readonly As4GatewayDbContext _dbContext;

    public SqliteAs4MessageAuditRepository(As4GatewayDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveAsync(As4MessageAuditRecord record, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(record);

        var entity = new As4MessageAuditEntity
        {
            Id = record.Id,
            ActionType = record.ActionType.ToString(),
            CreatedAtUtc = record.CreatedAtUtc,
            HttpStatusCode = record.HttpStatusCode,
            IsSuccess = record.IsSuccess,
            RequestXml = record.RequestXml,
            ResponseBody = record.ResponseBody,
            Status = record.Status,
            MessageId = record.MessageId,
            DocumentId = record.DocumentId,
            ErrorCode = record.ErrorCode,
            ErrorDescription = record.ErrorDescription,
            FaultReason = record.FaultReason
        };

        _dbContext.As4MessageAudits.Add(entity);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}