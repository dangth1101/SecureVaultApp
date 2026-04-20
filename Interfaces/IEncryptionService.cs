namespace SecureVaultApp.Interfaces;

public interface IEncryptionService
{
    public string Encrypt(string plainText);
    public string? Decrypt(string cipherText);
}