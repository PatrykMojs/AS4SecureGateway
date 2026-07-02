using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace AS4SecureGateway.TestEndpoint.Security;

public static class MultipartAs4RequestReader
{
    public static async Task<MultipartAs4Request> ReadAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        var contentType = request.ContentType;

        if (string.IsNullOrWhiteSpace(contentType))
            throw new InvalidOperationException("Missing Content-Type header.");

        if (!contentType.StartsWith("multipart/related", StringComparison.OrdinalIgnoreCase))
        {
            using var reader = new StreamReader(request.Body, Encoding.UTF8);

            return new MultipartAs4Request
            {
                EnvelopeXml = await reader.ReadToEndAsync(cancellationToken)
            };
        }

        var mediaType = MediaTypeHeaderValue.Parse(contentType);
        var boundary = HeaderUtilities.RemoveQuotes(mediaType.Boundary).Value;

        if (string.IsNullOrWhiteSpace(boundary))
            throw new InvalidOperationException("Multipart boundary was not found.");

        var multipartReader = new MultipartReader(boundary, request.Body);

        string? envelopeXml = null;
        var attachments = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);

        MultipartSection? section;

        while ((section = await multipartReader.ReadNextSectionAsync(cancellationToken)) is not null)
        {
            using var memoryStream = new MemoryStream();
            await section.Body.CopyToAsync(memoryStream, cancellationToken);

            var bytes = memoryStream.ToArray();
            var sectionContentType = section.ContentType ?? string.Empty;

            if (sectionContentType.Contains("application/soap+xml", StringComparison.OrdinalIgnoreCase))
            {
                envelopeXml = Encoding.UTF8.GetString(bytes);
                continue;
            }

            var contentId = GetContentId(section) ?? $"attachment-{attachments.Count + 1}";
            attachments[contentId] = bytes;
        }

        if (string.IsNullOrWhiteSpace(envelopeXml))
            throw new InvalidOperationException("SOAP envelope was not found in multipart request.");

        return new MultipartAs4Request
        {
            EnvelopeXml = envelopeXml,
            Attachments = attachments
        };
    }

    private static string? GetContentId(MultipartSection section)
    {
        if (!section.Headers.TryGetValue("Content-ID", out var values))
            return null;

        return values.ToString().Trim().Trim('<', '>');
    }
}