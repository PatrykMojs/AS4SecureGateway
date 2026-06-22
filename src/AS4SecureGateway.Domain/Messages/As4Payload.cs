namespace AS4SecureGateway.Domain.Messages;

public sealed class As4Payload
{
    public string ContentId { get; }
    public string MimeType { get; }
    public string CharacterSet { get; }
    public string? CompressionType { get; }
    public byte[] Content { get; }

    public As4Payload(
        string contentId,
        string mimeType,
        string characterSet,
        byte[] content,
        string? compressionType = null)
    {
        if (string.IsNullOrWhiteSpace(contentId))
            throw new ArgumentException("Content id cannot be empty.", nameof(contentId));

        if (string.IsNullOrWhiteSpace(mimeType))
            throw new ArgumentException("Mime type cannot be empty.", nameof(mimeType));

        if (string.IsNullOrWhiteSpace(characterSet))
            throw new ArgumentException("Character set cannot be empty.", nameof(characterSet));

        if (content.Length == 0)
            throw new ArgumentException("Payload content cannot be empty.", nameof(content));

        ContentId = contentId;
        MimeType = mimeType;
        CharacterSet = characterSet;
        CompressionType = compressionType;
        Content = content;
    }
}