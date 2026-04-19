namespace SecureVaultApp.Models;

public class Document
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; }
    public string EncryptedContent { get; set; }
    public string OwnerId { get; set; }
    public string Classification { get; set; } // Public, Internal, Confidential
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}