using AS4SecureGateway.Application.Abstractions.Messaging;
using AS4SecureGateway.Application.Messaging;

namespace AS4SecureGateway.Application.UseCases.DispatchAs4Message;

public sealed class DispatchAs4MessageHandler
{
    private readonly As4MessageMetadataFactory _metadataFactory;
    private readonly IAs4BusinessBodyFactory _businessBodyFactory;
    private readonly IAs4EnvelopeFactory _envelopeFactory;
    private readonly IAs4SecurityPipeline _securityPipeline;
    private readonly IAs4TransportClient _transportClient;

    public DispatchAs4MessageHandler(
        As4MessageMetadataFactory metadataFactory,
        IAs4BusinessBodyFactory businessBodyFactory,
        IAs4EnvelopeFactory envelopeFactory,
        IAs4SecurityPipeline securityPipeline,
        IAs4TransportClient transportClient
    )
    {
        _metadataFactory = metadataFactory;
        _businessBodyFactory = businessBodyFactory;
        _envelopeFactory = envelopeFactory;
        _securityPipeline = securityPipeline;
        _transportClient = transportClient;
    }

    public async Task<DispatchAs4MessageResult> HandleAsync(DispatchAs4MessageCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        ValidateCommand(command);

        var bodyXml = _businessBodyFactory.CreateBody(command);

        var metadata = _metadataFactory.Create(
            command.ActionType,
            command.SecurityOptions,
            command.SenderPartyId,
            command.SenderRole,
            command.ReceiverPartyId);

        var payloadHref = command.SecurityOptions.EnableCompression
            ? As4MessageDefaults.PayloadHref
            : string.Empty;

        var envelope = _envelopeFactory.CreateEnvelope(metadata, payloadHref);

        var preparedMessage = await _securityPipeline.PrepareAsync(
            envelope,
            bodyXml,
            command.SecurityOptions,
            cancellationToken);

        var transportResult = await _transportClient.SendAsync(preparedMessage, cancellationToken);

        return new DispatchAs4MessageResult
        {
            ActionType = command.ActionType,
            RequestXml = preparedMessage.Envelope.OuterXml,
            StatusCode = transportResult.StatusCode,
            ResponseBody = transportResult.ResponseBody
        };
    }

    private static void ValidateCommand(DispatchAs4MessageCommand command)
    {
        if(string.IsNullOrWhiteSpace(command.SenderPartyId))
           throw new InvalidOperationException("SenderPartyId is required.");

        if (string.IsNullOrWhiteSpace(command.SenderRole))
            throw new InvalidOperationException("SenderRole is required.");

        if (string.IsNullOrWhiteSpace(command.ReceiverPartyId))
            throw new InvalidOperationException("ReceiverPartyId is required.");

        if (command.ActionType == As4ActionType.SendMessage &&
            string.IsNullOrWhiteSpace(command.PayloadXml))
        {
            throw new InvalidOperationException("PayloadXml is required for SendMessage.");
        }

        if (command.ActionType == As4ActionType.DequeueMessage &&
            string.IsNullOrWhiteSpace(command.DocumentReferenceNumber))
        {
            throw new InvalidOperationException("DocumentReferenceNumber is required for DequeueMessage.");
        } 
    }
}