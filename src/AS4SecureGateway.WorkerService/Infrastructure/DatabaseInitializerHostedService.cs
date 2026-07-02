using AS4SecureGateway.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AS4SecureGateway.WorkerService.Infrastructure;

public sealed class DatabaseInitializerHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseInitializerHostedService> _logger;

    public DatabaseInitializerHostedService(IServiceProvider serviceProvider, ILogger<DatabaseInitializerHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<As4GatewayDbContext>();

        _logger.LogInformation("Applying SQLite database migrations...");

        await dbContext.Database.MigrateAsync(cancellationToken);

        _logger.LogInformation("SQLite database is ready.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}