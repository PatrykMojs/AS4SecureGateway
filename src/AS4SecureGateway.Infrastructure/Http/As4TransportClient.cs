using System.Net.Http.Headers;
using System.Text;
using AS4SecureGateway.Application.Abstractions.Messaging;
using AS4SecureGateway.Infrastructure.Responses;
using Microsoft.Extensions.Options;

namespace AS4SecureGateway.Infrastructure.Http;

public sealed class As4TransportClient : IAs4TransportClient
{
    private const string SoapRootContentId = "root.message@localhost";

    private readonly HttpClient _httpClient;
    private readonly As4TransportOptions _options;
    private readonly SecuredAs4ResponseProcessor _securedResponseProcessor;

    public As4TransportClient(
        HttpClient httpClient, 
        IOptions<As4TransportOptions> options,
        SecuredAs4ResponseProcessor securedResponseProcessor)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _securedResponseProcessor = securedResponseProcessor;

        if (_options.TimeoutSeconds > 0)
            _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
    }

    public async Task<As4TransportResult> SendAsync(PreparedAs4Message message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (string.IsNullOrWhiteSpace(_options.EndpointUrl))
            throw new InvalidOperationException("AS4 endpoint URL is not configured.");

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.EndpointUrl);

        request.Content = message.Attachments.Count > 0
            ? CreateMultipartContent(message)
            : CreateSoapContent(message);

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);

        var responseContentType = response.Content.Headers.ContentType?.ToString() ?? string.Empty;

        var responseBody = responseContentType.StartsWith(
            "multipart/related",
            StringComparison.OrdinalIgnoreCase)
                ? _securedResponseProcessor.Process(
                    await response.Content.ReadAsByteArrayAsync(cancellationToken),
                    responseContentType)
                : await response.Content.ReadAsStringAsync(cancellationToken);

        return new As4TransportResult
        {
            StatusCode = response.StatusCode,
            ResponseBody = responseBody
        };
    }

    private static HttpContent CreateSoapContent(PreparedAs4Message message)
    {
        var xml = message.Envelope.OuterXml;

        var content = new StringContent(xml, Encoding.UTF8, "application/soap+xml");

        return content;
    }

    private static HttpContent CreateMultipartContent(PreparedAs4Message message)
    {
        var boundary = "----as4-boundary-" + Guid.NewGuid().ToString("N");

        var multipart = new MultipartContent("related", boundary);

        multipart.Headers.ContentType ??= new MediaTypeHeaderValue("multipart/related");
        multipart.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("type", "\"application/soap+xml\""));
        multipart.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("start", $"\"<{SoapRootContentId}>\""));

        var soapContent = new StringContent(message.Envelope.OuterXml, Encoding.UTF8, "application/soap+xml");

        soapContent.Headers.TryAddWithoutValidation("Content-ID", $"<{SoapRootContentId}>");
        soapContent.Headers.TryAddWithoutValidation("Content-Transfer-Encoding", "8bit");

        multipart.Add(soapContent);

        foreach (var attachment in message.Attachments)
        {
            var attachmentContent = new ByteArrayContent(attachment.Value);

            attachmentContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            attachmentContent.Headers.TryAddWithoutValidation("Content-ID", $"<{attachment.Key}>");
            attachmentContent.Headers.TryAddWithoutValidation("Content-Transfer-Encoding", "binary");

            multipart.Add(attachmentContent);
        }

        return multipart;
    }
}