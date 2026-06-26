namespace AS4SecureGateway.Infrastructure.Http;

public sealed class As4TransportOptions
{
    public string EndpointUrl { get; init; } = string.Empty;
    public int TimeoutSeconds { get; init; } = 30;
}