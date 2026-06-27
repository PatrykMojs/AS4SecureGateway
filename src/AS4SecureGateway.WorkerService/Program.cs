using AS4SecureGateway.Application;
using AS4SecureGateway.Infrastructure;
using AS4SecureGateway.WorkerService;
using Serilog;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

var builder = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = "AS4 Secure Message Gateway";
    })
    .UseSerilog((context, services, loggerConfiguration) =>
    {
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddApplication();
        services.AddInfrastructure(context.Configuration);
        services.AddAs4WorkerService(context.Configuration);        
    });

try
{
    Log.Information("AS4 Secure Message Gateway starting...");

    await builder.Build().RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "AS4 Secure Message Gateway terminated unexpectedly.");
}
finally
{
    await Log.CloseAndFlushAsync();
}