namespace AS4SecureGateway.Application.Abstractions.Messaging;

public interface IAs4TransportClient
{
    Task<As4TransportResult> SendAsync(PreparedAs4Message message, CancellationToken cancellationToken);
}