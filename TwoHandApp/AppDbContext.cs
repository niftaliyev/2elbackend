using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TwoHandApp.Models;

namespace TwoHandApp;

public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<RolePermission> RolePermissions { get; set; }
    public ICollection<ApplicationUserRole> UserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<RolePermission>().Property(p => p.Permission)
            .HasConversion<string>();

        builder.Entity<RolePermission>()
            .HasOne(rp => rp.Role)
            .WithMany(r => r.Permissions)
            .HasForeignKey(rp => rp.RoleId);

        builder.Entity<Ad>()
           .HasOne(a => a.User)
           .WithMany(u => u.Ads)
           .HasForeignKey(a => a.UserId)
           .OnDelete(DeleteBehavior.Cascade); // Удалит все объявления, если удалить пользователя
    }
    public DbSet<Ad> Ads { get; set; }
    public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }

}
