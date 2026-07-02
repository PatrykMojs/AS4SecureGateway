using AS4SecureGateway.Application.UseCases.DispatchAs4Message;

namespace AS4SecureGateway.Application.Abstractions.Messaging;

public interface IAs4BusinessBodyFactory
{
    string CreateBody(DispatchAs4MessageCommand command);
}