namespace AS4SecureGateway.Domain.Messages;

public sealed class Party
{
    public string PartyId { get; }
    public string Role { get; }
    public string? PartyIdType { get; }

    public Party(string partyId, string role, string? partyIdType = null)
    {
        if (string.IsNullOrWhiteSpace(partyId))
            throw new ArgumentException("Party id cannot be empty.", nameof(partyId));

        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("Role cannot be empty.", nameof(role));

        PartyId = partyId;
        Role = role;
        PartyIdType = partyIdType;
    }
}