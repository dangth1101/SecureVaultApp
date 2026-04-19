using System.Security.Cryptography;
using SecureVaultApp.Interfaces;

namespace SecureVaultApp.Services.EncryptionServices;

public class AesEncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private HMACSHA256 CreateHmac() => new HMACSHA256(_key);

    public AesEncryptionService(IConfiguration configuration)
    {
        _key = Convert.FromBase64String(
            configuration["DataProtection:EncryptionKey"]
            ?? throw new InvalidOperationException("Encryption key not found in configuration"));
    }

    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var msEncrypt = new MemoryStream();
        msEncrypt.Write(aes.IV, 0, aes.IV.Length);

        using (var cs = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        using (var writer = new StreamWriter(cs))
        {
            writer.Write(plainText);
            writer.Flush();
        }

        var cipherBytes = msEncrypt.ToArray();
        using var hmac = CreateHmac();
        int signatureLength = hmac.HashSize / 8;
        var signature = hmac.ComputeHash(cipherBytes);

        using var msOutput = new MemoryStream();
        msOutput.Write(signature, 0, signatureLength);
        msOutput.Write(cipherBytes, 0, cipherBytes.Length);

        return Convert.ToBase64String(msOutput.ToArray());
    }

    public string Decrypt(string cipherText)
    {
        var buffer = Convert.FromBase64String(cipherText);

        using var hmac = CreateHmac();
        int signatureLength = hmac.HashSize / 8;
        var signature = buffer[..signatureLength];
        
        var cipherBytes = buffer[signatureLength..];
        var expectedSignature = hmac.ComputeHash(cipherBytes);

        if (!CryptographicOperations.FixedTimeEquals(signature, expectedSignature))
            throw new CryptographicException("Ciphertext has been tampered with.");

        using var aes = Aes.Create();
        aes.Key = _key;

        int ivLength = aes.BlockSize / 8;
        aes.IV = cipherBytes[..ivLength];

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var msDecrypt = new MemoryStream(cipherBytes[ivLength..]);
        using var cs = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var reader = new StreamReader(cs);

        return reader.ReadToEnd();
    }
}