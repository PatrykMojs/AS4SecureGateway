namespace AS4SecureGateway.Application.Abstractions.Cryptography;

public sealed record EncryptedAttachmentResult(
    byte[] EncryptedPayload,
    string EncryptedDataId,
    string EncryptedKeyId,
    string BinarySecurityTokenId,
    string ContentId);