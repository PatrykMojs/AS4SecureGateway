namespace AS4SecureGateway.Application.Messaging;

public sealed class As4MessageMetadataFactory
{
    private const string DefaultService = "DemoMarketMessaging";
    private const string DefaultReceiverRole = "Receiver";

    public As4MessageMetadata Create(
        As4ActionType actionType,
        SecurityProcessingOptions securityOptions,
        string senderPartyId,
        string senderRole,
        string receiverPartyId)
    {
        ArgumentNullException.ThrowIfNull(securityOptions);

        if (string.IsNullOrWhiteSpace(senderPartyId))
            throw new ArgumentException("Sender party id cannot be empty.", nameof(senderPartyId));

        if (string.IsNullOrWhiteSpace(senderRole))
            throw new ArgumentException("Sender role cannot be empty.", nameof(senderRole));

        if (string.IsNullOrWhiteSpace(receiverPartyId))
            throw new ArgumentException("Receiver party id cannot be empty.", nameof(receiverPartyId));

        var action = GetActionName(actionType);
        var suffix = securityOptions.GetAgreementSuffix();

        return new As4MessageMetadata
        {
            SenderPartyId = senderPartyId,
            SenderRole = senderRole,

            ReceiverPartyId = receiverPartyId,
            ReceiverRole = DefaultReceiverRole,

            Service = DefaultService,
            Action = action,
            AgreementRef = CreateAgreementRef(actionType, action, suffix, securityOptions),
            ConversationId = CreateConversationId(),

            MimeType = securityOptions.EnableCompression ? "application/xml" : null,
            CharacterSet = securityOptions.EnableCompression ? "utf-8" : null,
            CompressionType = securityOptions.EnableCompression ? "application/gzip" : null
        };
    }

    private static string GetActionName(As4ActionType actionType)
    {
        return actionType switch
        {
            As4ActionType.SendMessage => "SendMessage",
            As4ActionType.PeekMessage => "PeekMessage.Request",
            As4ActionType.DequeueMessage => "DequeueMessage",
            _ => throw new InvalidOperationException($"Unsupported action type: {actionType}")
        };
    }

    private static string CreateAgreementRef(
        As4ActionType actionType,
        string action,
        string suffix,
        SecurityProcessingOptions securityOptions)
    {
        var shouldSkipSuffixForDequeue =
            actionType == As4ActionType.DequeueMessage &&
            !securityOptions.EnableCompression &&
            !(securityOptions.EnableEncryption && securityOptions.EnableSignature);

        if (shouldSkipSuffixForDequeue)
            return $"urn:demo:as4:agreement:{action}";

        return $"urn:demo:as4:agreement:{action}{suffix}";
    }

    private static string CreateConversationId()
    {
        return $"{DateTime.UtcNow:yyyy}-{Guid.NewGuid():N}";
    }
}