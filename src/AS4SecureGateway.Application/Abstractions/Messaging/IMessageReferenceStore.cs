namespace AS4SecureGateway.Application.Abstractions.Messaging;

public interface IMessageReferenceStore
{
    Task SaveAsync(string messageDomain, string documentReferenceNumber, CancellationToken cancellationToken);
    Task<string?> GetAsync(string messageDomain, CancellationToken cancellationToken);
}