using Microsoft.AspNetCore.DataProtection;
using SecureVaultApp.Interfaces;

namespace SecureVaultApp.Services.EncryptionServices;
public class DataProtectionEncryptionService : IEncryptionService
{
    private readonly IDataProtector _protector;

    public DataProtectionEncryptionService(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("SecureVaultApp.DocumentProtection");
    }

    public string Encrypt(string plainText)
    {
        return _protector.Protect(plainText);
    }

    public string Decrypt(string cipherText)
    {

        return _protector.Unprotect(cipherText) ?? throw new InvalidOperationException("Decryption failed");
    }
}