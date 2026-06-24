namespace AS4SecureGateway.Infrastructure.Cryptography;

public static class SecurityAlgorithms
{
    public const string AttachmentContentSignatureTransform =
        "http://docs.oasis-open.org/wss/oasis-wss-SwAProfile-1.1#Attachment-Content-Signature-Transform";

    public const string AttachmentCiphertextTransform =
        "http://docs.oasis-open.org/wss/oasis-wss-SwAProfile-1.1#Attachment-Ciphertext-Transform";

    public const string AttachmentContentOnly =
        "http://docs.oasis-open.org/wss/oasis-wss-SwAProfile-1.1#Attachment-Content-Only";

    public const string RsaSha256 =
        "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";

    public const string RsaOaepMgf1p =
        "http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p";

    public const string Aes128Cbc =
        "http://www.w3.org/2001/04/xmlenc#aes128-cbc";

    public const string Aes256Cbc =
        "http://www.w3.org/2001/04/xmlenc#aes256-cbc";
}