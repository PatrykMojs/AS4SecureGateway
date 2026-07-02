namespace AS4SecureGateway.Domain.Messages;

public sealed class CollaborationInfo
{
    public string AgreementRef { get; }
    public string Service { get; }
    public string Action { get; }

    public CollaborationInfo(string agreementRef, string service, string action)
    {
        if (string.IsNullOrWhiteSpace(agreementRef))
            throw new ArgumentException("Agreement reference cannot be empty.", nameof(agreementRef));

        if (string.IsNullOrWhiteSpace(service))
            throw new ArgumentException("Service cannot be empty.", nameof(service));

        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action cannot be empty.", nameof(action));

        AgreementRef = agreementRef;
        Service = service;
        Action = action;
    }
}