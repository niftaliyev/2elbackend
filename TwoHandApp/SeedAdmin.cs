using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TwoHandApp.Models;

namespace TwoHandApp;

public static class SeedAdmin
{
    public static async Task SeedAdminAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var adminRole = await roleManager.FindByNameAsync("Admin");
        string adminEmail = "admin";
        string adminPassword = "Admin@123";

        string adminRoleName = "Admin";

        if (!await roleManager.RoleExistsAsync(adminRoleName))
        {
            var role = new ApplicationRole { Name = adminRoleName };
            await roleManager.CreateAsync(role);

            foreach (Permission perm in Enum.GetValues<Permission>())
            {
                db.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    Permission = perm
                });
            }

            await db.SaveChangesAsync();
        }

        var user = await userManager.FindByEmailAsync(adminEmail);
        if (user == null)
        {
            user = new ApplicationUser { UserName = "admin", Email = adminEmail, EmailConfirmed = true };
            await userManager.CreateAsync(user, adminPassword);
            await userManager.AddToRoleAsync(user, adminRoleName);
        }

        // Добавить в базу permissions из enum если их нет
        foreach (Permission perm in Enum.GetValues(typeof(Permission)))
        {
            bool exists = await context.RolePermissions
                .AnyAsync(rp => rp.RoleId == adminRole.Id && rp.Permission == perm);

            if (!exists)
            {
                context.RolePermissions.Add(new RolePermission
                {
                    RoleId = adminRole.Id,
                    Permission = perm
                });
            }
        }
        await context.SaveChangesAsync();

    }
}
