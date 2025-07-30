using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TwoHandApp.Models;

namespace TwoHandApp;

public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<RolePermission> RolePermissions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<RolePermission>().Property(p => p.Permission)
            .HasConversion<string>();

        builder.Entity<RolePermission>()
            .HasOne(rp => rp.Role)
            .WithMany(r => r.Permissions)
            .HasForeignKey(rp => rp.RoleId);
    }
    public DbSet<Advertisement> Advertisements { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<PostPhoto> PostPhotos { get; set; }
}

public class Advertisement
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsPro { get; set; }
    public bool IsVip { get; set; }
    public DateTime? ProExpiryDate { get; set; }
    public DateTime? VipExpiryDate { get; set; }
    public string? Image { get; set; } = "default.jpg";
}

