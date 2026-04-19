using Microsoft.AspNetCore.Identity;
using SecureVaultApp.Models;

namespace SecureVaultApp.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        // Seed Roles
        string[] roles = { "Admin", "Manager", "Viewer" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Seed Admin User
        if (await userManager.FindByEmailAsync("admin@securevault.com") == null)
        {
            var admin = new IdentityUser
            {
                UserName = "admin",
                Email = "admin@securevault.com",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, "Admin@1234!");

            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }

        // Seed Sample Documents
        if (!context.Documents.Any())
        {
            context.Documents.AddRange(
                new Document
                {
                    Title = "Welcome Guide",
                    EncryptedContent = "placeholder-encrypted-content-1",
                    OwnerId = "seeded",
                    Classification = "Public"
                },
                new Document
                {
                    Title = "Company Policy",
                    EncryptedContent = "placeholder-encrypted-content-2",
                    OwnerId = "seeded",
                    Classification = "Internal"
                },
                new Document
                {
                    Title = "Q4 Financial Report",
                    EncryptedContent = "placeholder-encrypted-content-3",
                    OwnerId = "seeded",
                    Classification = "Confidential"
                }
            );

            await context.SaveChangesAsync();
        }
    }
}