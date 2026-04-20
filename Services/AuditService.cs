using Microsoft.AspNetCore.Http;
using SecureVaultApp.Data;
using SecureVaultApp.Helpers;
using SecureVaultApp.Interfaces;
using SecureVaultApp.Models;

namespace SecureVaultApp.Services;

public class AuditService : IAuditService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(string userId, string action, string resourceId, string ipAddress)
    {
        // Use IpAddressHelper if no IP was passed in
        var ip = ipAddress ?? IpAddressHelper.GetIpAddress(_httpContextAccessor.HttpContext!);

        var log = new AuditLog
        {
            UserId = userId,
            Action = action,
            ResourceId = resourceId,
            IpAddress = ip,
            Timestamp = DateTime.UtcNow
        };

        await _context.AuditLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }
}