namespace SecureVaultApp.Interfaces;

public interface IAuditService
{
    Task LogAsync(string userId, string action, string resourceId, string ipAddress);
}