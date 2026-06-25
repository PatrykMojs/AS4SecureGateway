using System.Xml;
using AS4SecureGateway.Application.Messaging;

namespace AS4SecureGateway.Application.Abstractions.Messaging;

public interface IAs4SecurityPipeline
{
    Task<PreparedAs4Message> PrepareAsync(
        XmlDocument envelope, 
        string businessBodyXml, 
        SecurityProcessingOptions securityOptions, 
        CancellationToken cancellationToken);
}