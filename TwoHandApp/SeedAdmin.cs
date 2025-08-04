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

        const string adminEmail = "admin";
        const string adminPassword = "123456";
        const string adminRoleName = "Admin";

        ApplicationRole adminRole = await roleManager.FindByNameAsync(adminRoleName);
        if (adminRole == null)
        {
            adminRole = new ApplicationRole { Name = adminRoleName };
            var roleResult = await roleManager.CreateAsync(adminRole);

            if (!roleResult.Succeeded)
                throw new Exception("Не удалось создать роль Admin");

            // Добавляем все permissions к роли
            foreach (Permission perm in Enum.GetValues<Permission>())
            {
                context.RolePermissions.Add(new RolePermission
                {
                    RoleId = adminRole.Id,
                    Permission = perm
                });
            }

            await context.SaveChangesAsync();
        }

        var user = await userManager.FindByEmailAsync(adminEmail);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = "admin",
                Email = adminEmail,
                EmailConfirmed = true
            };

            var userCreateResult = await userManager.CreateAsync(user, adminPassword);
            if (!userCreateResult.Succeeded)
                throw new Exception("Не удалось создать пользователя admin");

            await userManager.AddToRoleAsync(user, adminRoleName);
        }

        // Safety check (если какие-то permissions были добавлены позже)
        foreach (Permission perm in Enum.GetValues<Permission>())
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

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var rolesWithPermissions = new Dictionary<string, List<Permission>>
        {
            ["User"] = new()
            {
                Permission.Ads_Create,
                Permission.Ads_Edit_Own,
                Permission.Ads_Delete_Own,
                Permission.Ads_View,
                Permission.Ads_Report
            },
            ["Moderator"] = new()
            {
                Permission.Ads_Edit_Any,
                Permission.Ads_Delete_Any,
                Permission.Comments_View,
                Permission.Comments_Delete,
                Permission.Reports_View,
                Permission.Reports_Resolve,
                Permission.Users_Ban,
                Permission.Users_Unban
            },
            ["Admin"] = new()
            {
                Permission.Categories_Manage,
                Permission.Cities_Manage,
                Permission.Users_View,
                Permission.Users_ManageRoles,
                Permission.Roles_Create,
                Permission.Roles_Edit,
                Permission.Roles_Delete
            },
            ["SuperAdmin"] = Enum.GetValues<Permission>().ToList()
        };

        foreach (var (roleName, permissions) in rolesWithPermissions)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                role = new ApplicationRole { Name = roleName };
                await roleManager.CreateAsync(role);
            }

            foreach (var perm in permissions)
            {
                var exists = await db.RolePermissions.AnyAsync(rp => rp.RoleId == role.Id && rp.Permission == perm);
                if (!exists)
                {
                    db.RolePermissions.Add(new RolePermission
                    {
                        RoleId = role.Id,
                        Permission = perm
                    });
                }
            }
        }

        await db.SaveChangesAsync();
    }
}