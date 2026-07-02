using System.Text;
using AS4SecureGateway.Application.Abstractions.Mime;
using MimeKit;

namespace AS4SecureGateway.Infrastructure.Mime;

public sealed class MimeAttachmentParser : IMimeAttachmentParser
{
    public IReadOnlyDictionary<string, byte[]> ParseAttachments(string mimeMessage)
    {
        if (string.IsNullOrWhiteSpace(mimeMessage))
            throw new ArgumentException("MIME message cannot be empty.", nameof(mimeMessage));

        var attachments = new Dictionary<string, byte[]>();
        var mimeBytes = Encoding.UTF8.GetBytes(mimeMessage);

        using var stream = new MemoryStream(mimeBytes);
        var message = MimeMessage.Load(stream);

        foreach (var entity in message.BodyParts)
        {
            if (entity is not MimePart part || string.IsNullOrWhiteSpace(part.ContentId))
                continue;

            using var contentStream = new MemoryStream();
            part.Content.DecodeTo(contentStream);

            var contentId = part.ContentId.Trim('<', '>');
            attachments[contentId] = contentStream.ToArray();
        }

        return attachments;
    }
}