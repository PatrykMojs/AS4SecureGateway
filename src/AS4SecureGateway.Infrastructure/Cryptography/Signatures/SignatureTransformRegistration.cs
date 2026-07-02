using System.Security.Cryptography;

namespace AS4SecureGateway.Infrastructure.Cryptography.Signatures;

public static class SignatureTransformRegistration
{
    private static int _registered;

    public static void Register()
    {
        if(Interlocked.Exchange(ref _registered, 1) == 1)
            return;

        CryptoConfig.AddAlgorithm(
            typeof(AttachmentContentSignatureTransform),
            AttachmentContentSignatureTransform.AlgorithmUri);
    }
}