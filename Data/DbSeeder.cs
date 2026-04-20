using System.Security.Claims;
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

        // ===== SEED ROLES =====
        string[] roles = { "Admin", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // ===== SEED ADMIN USER =====
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
            {
                await userManager.AddToRoleAsync(admin, "Admin");
                await userManager.AddClaimAsync(admin, new Claim("department", "Management"));
            }
        }

        // ===== SEED FINANCE USER =====
        if (await userManager.FindByEmailAsync("finance@securevault.com") == null)
        {
            var financeUser = new IdentityUser
            {
                UserName = "financeuser",
                Email = "finance@securevault.com",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(financeUser, "Finance@1234!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(financeUser, "User");
                await userManager.AddClaimAsync(financeUser, new Claim("department", "Finance"));
            }
        }

        // ===== SEED ENGINEERING USER =====
        if (await userManager.FindByEmailAsync("engineering@securevault.com") == null)
        {
            var engineeringUser = new IdentityUser
            {
                UserName = "engineeringuser",
                Email = "engineering@securevault.com",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(engineeringUser, "Engineering@1234!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(engineeringUser, "User");
                await userManager.AddClaimAsync(engineeringUser, new Claim("department", "Engineering"));
            }
        }

        // ===== SEED HR USER =====
        if (await userManager.FindByEmailAsync("hr@securevault.com") == null)
        {
            var hrUser = new IdentityUser
            {
                UserName = "hruser",
                Email = "hr@securevault.com",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(hrUser, "Hr@1234!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(hrUser, "User");
                await userManager.AddClaimAsync(hrUser, new Claim("department", "HR"));
            }
        }

        // ===== SEED SAMPLE DOCUMENTS =====
        if (!context.Documents.Any())
        {
            context.Documents.AddRange(
                new Document
                {
                    Title = "Welcome Guide",
                    EncryptedContent = "placeholder-encrypted-content-1",
                    OwnerId = "seeded",
                    Classification = "Public",
                    Department = "Management"
                },
                new Document
                {
                    Title = "Q4 Financial Report",
                    EncryptedContent = "placeholder-encrypted-content-2",
                    OwnerId = "seeded",
                    Classification = "Confidential",
                    Department = "Finance"
                },
                new Document
                {
                    Title = "System Architecture",
                    EncryptedContent = "placeholder-encrypted-content-3",
                    OwnerId = "seeded",
                    Classification = "Internal",
                    Department = "Engineering"
                },
                new Document
                {
                    Title = "Employee Handbook",
                    EncryptedContent = "placeholder-encrypted-content-4",
                    OwnerId = "seeded",
                    Classification = "Internal",
                    Department = "HR"
                }
            );

            await context.SaveChangesAsync();
        }
    }
}