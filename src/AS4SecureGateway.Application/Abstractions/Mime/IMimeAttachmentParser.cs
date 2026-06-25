namespace AS4SecureGateway.Application.Abstractions.Mime;

public interface IMimeAttachmentParser
{
    IReadOnlyDictionary<string, byte[]> ParseAttachments(string mimeMessage);
}