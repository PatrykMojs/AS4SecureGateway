namespace AS4SecureGateway.Application.Messaging;

public sealed class SecurityProcessingOptions
{
    public bool EnableCompression { get; init; }
    public bool EnableEncryption { get; init; }
    public bool EnableSignature { get; init; }

    public string GetAgreementSuffix()
    {
        var parts = new List<string>();

        if (EnableCompression)
            parts.Add("Compression");

        if (EnableEncryption)
            parts.Add("Encryption");

        if (EnableSignature)
            parts.Add("Signature");

        return parts.Count == 0
            ? string.Empty
            : ":" + string.Join(":", parts);
    }

    public string GetEndpointPathSegment()
    {
        if (EnableCompression && EnableEncryption && EnableSignature)
            return "CompressionEncryptionSignature";

        if (EnableEncryption && EnableSignature)
            return "EncryptionSignature";

        if (EnableCompression)
            return "Compression";

        if (EnableEncryption)
            return "Encryption";

        if (EnableSignature)
            return "Signature";

        return string.Empty;
    }
}