using System.Security.Cryptography;
using System.Text;
using SecureVaultApp.Interfaces;

namespace SecureVaultApp.Services.EncryptionServices;

public class RsaEncryptionService : IEncryptionService
{
    private readonly RSA _rsaPublic;
    private readonly RSA _rsaPrivate;

    public RsaEncryptionService(IConfiguration configuration)
    {
        _rsaPublic = RSA.Create();
        _rsaPublic.ImportFromPem(configuration["DataProtection:RsaPublicKey"]
            ?? throw new InvalidOperationException("RSA public key not found in configuration"));

        _rsaPrivate = RSA.Create();
        _rsaPrivate.ImportFromPem(configuration["DataProtection:RsaPrivateKey"]
            ?? throw new InvalidOperationException("RSA private key not found in configuration"));
    }

    public string Encrypt(string plainText)
    {
        var data = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = _rsaPublic.Encrypt(data, RSAEncryptionPadding.OaepSHA256);
        return Convert.ToBase64String(cipherBytes);
    }

    public string Decrypt(string cipherText)
    {
        var cipherBytes = Convert.FromBase64String(cipherText);
        var data = _rsaPrivate.Decrypt(cipherBytes, RSAEncryptionPadding.OaepSHA256);
        return Encoding.UTF8.GetString(data);
    }
}