namespace SecureVaultApp.Interfaces;

interface IEncryptionService
{
    public string Encrypt(string plainText);
    public string Decrypt(string cipherText);
}