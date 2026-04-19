using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SecureVaultApp.Models;

namespace SecureVaultApp.Data;

public class AppDbContext : IdentityDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Document> Documents { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
}