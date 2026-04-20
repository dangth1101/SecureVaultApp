using Microsoft.EntityFrameworkCore;
using SecureVaultApp.Data;
using SecureVaultApp.Interfaces;
using SecureVaultApp.Services;
using SecureVaultApp.Services.EncryptionServices;

namespace SecureVaultApp.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddSecureVaultServices(
        this IServiceCollection services
       )
    {
        // Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("SecureVaultDb"));

        // HTTP Context
        services.AddHttpContextAccessor();

        // Custom Services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IEncryptionService, AesEncryptionService>();
        services.AddScoped<IAuditService, AuditService>();

        return services;
    }
}