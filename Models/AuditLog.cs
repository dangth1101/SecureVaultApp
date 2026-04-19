namespace SecureVaultApp.Models;

public class AuditLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; }
    public string Action { get; set; }
    public string ResourceId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string IpAddress { get; set; }
}