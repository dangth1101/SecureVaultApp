using SecureVaultApp.Data;
using SecureVaultApp.Interfaces;
using SecureVaultApp.Models;

namespace SecureVaultApp.Services;

public class AuditService : IAuditService
{
    private readonly AppDbContext _context;

    public AuditService(AppDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(string userId, string action, string resourceId, string ipAddress)
    {
        var log = new AuditLog
        {
            UserId = userId,
            Action = action,
            ResourceId = resourceId,
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow
        };

        await _context.AuditLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }
}