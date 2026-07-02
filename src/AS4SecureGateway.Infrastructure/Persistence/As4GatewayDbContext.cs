using AS4SecureGateway.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace AS4SecureGateway.Infrastructure.Persistence;

public sealed class As4GatewayDbContext : DbContext
{
    public As4GatewayDbContext(DbContextOptions<As4GatewayDbContext> options) : base(options)
    {
    }

    public DbSet<As4MessageAuditEntity> As4MessageAudits => Set<As4MessageAuditEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var audit = modelBuilder.Entity<As4MessageAuditEntity>();

        audit.ToTable("As4MessageAudits");

        audit.HasKey(x => x.Id);

        audit.Property(x => x.ActionType)
            .HasMaxLength(50)
            .IsRequired();

        audit.Property(x => x.CreatedAtUtc)
            .IsRequired();

        audit.Property(x => x.HttpStatusCode)
            .IsRequired();

        audit.Property(x => x.IsSuccess)
            .IsRequired();

        audit.Property(x => x.RequestXml)
            .IsRequired();

        audit.Property(x => x.ResponseBody)
            .IsRequired();

        audit.Property(x => x.Status)
            .HasMaxLength(100);

        audit.Property(x => x.MessageId)
            .HasMaxLength(200);

        audit.Property(x => x.DocumentId)
            .HasMaxLength(100);

        audit.Property(x => x.ErrorCode)
            .HasMaxLength(100);

        audit.Property(x => x.ErrorDescription)
            .HasMaxLength(1000);

        audit.Property(x => x.FaultReason)
            .HasMaxLength(1000);

        audit.HasIndex(x => x.CreatedAtUtc);
        audit.HasIndex(x => x.ActionType);
        audit.HasIndex(x => x.MessageId);
        audit.HasIndex(x => x.DocumentId);
    }
}