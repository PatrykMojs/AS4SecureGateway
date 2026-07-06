using System.Net;
using AS4SecureGateway.Application.UseCases.DispatchAs4Message;
using FluentAssertions;

namespace AS4SecureGateway.Application.Tests.UseCases;

public sealed class DispatchAs4MessageResultTests
{
    [Theory]
    [InlineData(HttpStatusCode.OK, true)]
    [InlineData(HttpStatusCode.Created, true)]
    [InlineData(HttpStatusCode.Accepted, true)]
    [InlineData(HttpStatusCode.MultipleChoices, false)]
    [InlineData(HttpStatusCode.BadRequest, false)]
    [InlineData(HttpStatusCode.InternalServerError, false)]
    public void IsSuccessStatusCode_ShouldReturnTrueOnlyFor2xxStatusCodes(
        HttpStatusCode statusCode,
        bool expected)
    {
        var result = new DispatchAs4MessageResult
        {
            StatusCode = statusCode
        };

        result.IsSuccessStatusCode.Should().Be(expected);
    }
}