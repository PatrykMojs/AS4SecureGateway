using AS4SecureGateway.Application.Messaging;
using AS4SecureGateway.Application.UseCases.DispatchAs4Message;
using Microsoft.Extensions.DependencyInjection;

namespace AS4SecureGateway.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<As4MessageMetadataFactory>();
        services.AddTransient<DispatchAs4MessageHandler>();

        return services;
    }
}