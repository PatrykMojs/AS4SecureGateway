using System.Text;
using System.Text.RegularExpressions;

namespace AS4SecureGateway.Infrastructure.Responses;

public static class MultipartAs4ResponseReader
{
    public static MultipartAs4Response Read(byte[] responseBytes, string contentType)
    {
        if (responseBytes.Length == 0)
            throw new InvalidOperationException("Multipart response is empty.");

        var boundary = ExtractBoundary(contentType);

        var raw = Encoding.Latin1.GetString(responseBytes);

        var delimiter = "--" + boundary;

        var rawParts = raw
            .Split(delimiter, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.TrimStart('\r', '\n'))
            .Where(x => !x.StartsWith("--", StringComparison.Ordinal))
            .ToList();

        string? envelopeXml = null;
        var attachments = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);

        foreach (var rawPart in rawParts)
        {
            var headerEndIndex = rawPart.IndexOf("\r\n\r\n", StringComparison.Ordinal);

            if (headerEndIndex < 0)
                continue;

            var headers = rawPart[..headerEndIndex];
            var body = rawPart[(headerEndIndex + 4)..];

            if (body.EndsWith("\r\n", StringComparison.Ordinal))
                body = body[..^2];

            var bodyBytes = Encoding.Latin1.GetBytes(body);
            var partContentType = GetHeaderValue(headers, "Content-Type") ?? string.Empty;

            if (partContentType.Contains("application/soap+xml", StringComparison.OrdinalIgnoreCase))
            {
                envelopeXml = Encoding.UTF8.GetString(bodyBytes);
                continue;
            }

            var contentId = GetHeaderValue(headers, "Content-ID")
                ?.Trim()
                .Trim('<', '>');

            if (string.IsNullOrWhiteSpace(contentId))
                contentId = $"attachment-{attachments.Count + 1}";

            attachments[contentId] = bodyBytes;
        }

        if (string.IsNullOrWhiteSpace(envelopeXml))
            throw new InvalidOperationException("SOAP envelope was not found in multipart response.");

        return new MultipartAs4Response
        {
            EnvelopeXml = envelopeXml,
            Attachments = attachments
        };
    }

    private static string ExtractBoundary(string contentType)
    {
        var match = Regex.Match(contentType, @"boundary=""?(?<boundary>[^"";]+)""?", RegexOptions.IgnoreCase);

        if (!match.Success)
            throw new InvalidOperationException("Multipart boundary was not found in Content-Type header.");

        return match.Groups["boundary"].Value;
    }

    private static string? GetHeaderValue(string headers, string headerName)
    {
        using var reader = new StringReader(headers);

        string? line;

        while ((line = reader.ReadLine()) is not null)
        {
            var separatorIndex = line.IndexOf(':');

            if (separatorIndex < 0)
                continue;

            var name = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim();

            if (name.Equals(headerName, StringComparison.OrdinalIgnoreCase))
                return value;
        }

        return null;
    }
}