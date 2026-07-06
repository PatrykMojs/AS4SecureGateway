using AS4SecureGateway.Domain.Messages;
using FluentAssertions;

namespace AS4SecureGateway.Domain.Tests.Messages;

public sealed class PartyTests
{
    [Fact]
    public void Constructor_WithValidValues_ShouldCreateParty()
    {
        var party = new Party("party-id", "sender-role", "urn:oasis:names:tc:ebcore:partyid-type:unregistered");

        party.PartyId.Should().Be("party-id");
        party.Role.Should().Be("sender-role");
        party.PartyIdType.Should().Be("urn:oasis:names:tc:ebcore:partyid-type:unregistered");
    }

    [Theory]
    [InlineData("", "role", "partyId")]
    [InlineData("party-id", "", "role")]
    public void Constructor_WhenRequiredValueIsEmpty_ShouldThrowArgumentException(string partyId, string role, string expectedParamName)
    {
        var act = () => new Party(partyId, role);

        act.Should().Throw<ArgumentException>()
            .Where(exception => exception.ParamName == expectedParamName);
    }
}
