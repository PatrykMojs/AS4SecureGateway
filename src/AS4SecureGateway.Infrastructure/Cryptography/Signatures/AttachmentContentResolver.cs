using System.Net;
using System.Security.Cryptography;
using System.Xml;

namespace AS4SecureGateway.Infrastructure.Cryptography.Signatures;

public sealed class AttachmentContentResolver : XmlResolver
{
    private readonly IReadOnlyDictionary<string, byte[]> _attachments;

    public AttachmentContentResolver(IReadOnlyDictionary<string, byte[]> attachments)
    {
        _attachments = attachments;
    }

    public override ICredentials? Credentials
    {
        set { }
    }

    public override object? GetEntity(Uri absoluteUri, string? role, Type? ofObjectToReturn)
    {
        ArgumentNullException.ThrowIfNull(absoluteUri);

        var contentId = NormalizeContentId(absoluteUri.ToString());

        if(_attachments.TryGetValue(contentId, out var content))
            return new MemoryStream(content, writable: false);

        if (_attachments.TryGetValue($"<{contentId}>", out var contentWithBrackets))
            return new MemoryStream(contentWithBrackets, writable: false);

        throw new CryptographicException($"Unable to resolve attachment URI: {absoluteUri}");
    }

    public override Uri ResolveUri(Uri? baseUri, string? relativeUri)
    {
        if(!string.IsNullOrWhiteSpace(relativeUri) 
            && relativeUri.StartsWith("cid:", StringComparison.OrdinalIgnoreCase))
        {
            return new Uri(relativeUri, UriKind.RelativeOrAbsolute);
        }

        return base.ResolveUri(baseUri, relativeUri);
    }

    private static string NormalizeContentId(string uri)
    {
        var contentId = uri.StartsWith("cid:", StringComparison.OrdinalIgnoreCase)
            ? uri[4..]
            : uri;

        return contentId.Trim('<', '>');
    }
}