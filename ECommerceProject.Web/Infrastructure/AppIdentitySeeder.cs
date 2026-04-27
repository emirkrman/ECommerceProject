using ECommerceProject.Data.Context;
using ECommerceProject.Entity.Common;
using ECommerceProject.Entity.Concrete;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECommerceProject.Web.Infrastructure;

public static class AppIdentitySeeder
{
    public const string AdminEmail = "admin@ecommerce.local";
    public const string AdminPassword = "Admin123!";
    public const string AdminFullName = "System Admin";

    public static async Task SeedAsync(AppDbContext context)
    {
        var passwordHasher = new PasswordHasher<AppUser>();
        var adminEmail = AdminEmail.ToLowerInvariant();

        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);

        if (adminUser == null)
        {
            adminUser = new AppUser
            {
                FullName = AdminFullName,
                Email = adminEmail,
                Role = AppRoles.Admin,
                CreatedDate = DateTime.UtcNow
            };

            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, AdminPassword);

            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
            return;
        }

        var requiresUpdate = false;

        if (!string.Equals(adminUser.Role, AppRoles.Admin, StringComparison.Ordinal))
        {
            adminUser.Role = AppRoles.Admin;
            requiresUpdate = true;
        }

        if (requiresUpdate)
        {
            adminUser.UpdatedDate = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }
    }
}
