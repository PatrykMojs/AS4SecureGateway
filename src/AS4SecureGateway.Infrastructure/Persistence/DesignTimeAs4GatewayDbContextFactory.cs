using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AS4SecureGateway.Infrastructure.Persistence;

public sealed class DesignTimeAs4GatewayDbContextFactory : IDesignTimeDbContextFactory<As4GatewayDbContext>
{
    public As4GatewayDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<As4GatewayDbContext>();

        optionsBuilder.UseSqlite("Data Source=./data/as4-gateway.db");

        return new As4GatewayDbContext(optionsBuilder.Options);
    }
}