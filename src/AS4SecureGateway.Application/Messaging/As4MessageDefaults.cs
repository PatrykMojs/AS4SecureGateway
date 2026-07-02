namespace AS4SecureGateway.Application.Messaging;

public static class As4MessageDefaults
{
    public const string PayloadContentId = "ebms3-payload@localhost";
    public const string PayloadHref = "cid:" + PayloadContentId;
    public const string SoapContentType = "application/soap+xml";
    public const string MultipartContentType = "multipart/related";
}