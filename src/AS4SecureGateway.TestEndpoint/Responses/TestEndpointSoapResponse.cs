namespace AS4SecureGateway.TestEndpoint.Responses;

public sealed record TestEndpointSoapResponse(int StatusCode, string ContentType, string Body);