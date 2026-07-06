using AS4SecureGateway.Domain.Messages;
using FluentAssertions;

namespace AS4SecureGateway.Domain.Tests.Messages;

public sealed class CollaborationInfoTests
{
    [Fact]
    public void Constructor_WithValidValues_ShouldCreateCollaborationInfo()
    {
        var collaborationInfo = new CollaborationInfo("urn:demo:as4:agreement:SendMessage", "bdx:noprocess", "SendMessage");

        collaborationInfo.AgreementRef.Should().Be("urn:demo:as4:agreement:SendMessage");
        collaborationInfo.Service.Should().Be("bdx:noprocess");
        collaborationInfo.Action.Should().Be("SendMessage");
    }

    [Theory]
    [InlineData("", "service", "action", "agreementRef")]
    [InlineData("agreement", "", "action", "service")]
    [InlineData("agreement", "service", "", "action")]
    public void Constructor_WhenRequiredValueIsEmpty_ShouldThrowArgumentException(
        string agreementRef,
        string service,
        string action,
        string expectedParamName)
    {
        var act = () => new CollaborationInfo(agreementRef, service, action);

        act.Should().Throw<ArgumentException>()
            .Where(exception => exception.ParamName == expectedParamName);
    }
}