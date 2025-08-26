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

        // Ad → User
        builder.Entity<Ad>()
            .HasOne(a => a.User)
            .WithMany(u => u.Ads)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ad → AdType
        builder.Entity<Ad>()
            .HasOne(a => a.AdType)
            .WithMany(at => at.Ads)
            .HasForeignKey(a => a.AdTypeId)
            .OnDelete(DeleteBehavior.Restrict); // обычно не cascade

        // Ad → Category
        builder.Entity<Ad>()
            .HasOne(a => a.Category)
            .WithMany(c => c.Ads)
            .HasForeignKey(a => a.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ad → City
        builder.Entity<Ad>()
            .HasOne(a => a.City)
            .WithMany(c => c.Ads)
            .HasForeignKey(a => a.CityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<FavouriteAd>()
            .HasOne(f => f.User)
            .WithMany(u => u.FavouriteAds)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<FavouriteAd>()
            .HasOne(f => f.Ad)
            .WithMany(a => a.Favourites)
            .HasForeignKey(f => f.AdId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Seed данные для FK
        builder.Entity<AdType>().HasData(
            new AdType { Id = 1, Name = "Business" },
            new AdType { Id = 2, Name = "Personal" }
        );

        builder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Electronics" },
            new Category { Id = 2, Name = "Furniture" }
        );

        builder.Entity<City>().HasData(
            new City { Id = 1, Name = "Baku" },
            new City { Id = 2, Name = "Ganja" }
        );
        
    }

    public DbSet<Ad> Ads { get; set; } = default!;
    public DbSet<AdImage> AdImages { get; set; } = default!;
    public DbSet<Category> Categories { get; set; } = default!;
    public DbSet<City> Cities { get; set; } = default!;
    public DbSet<AdType> AdTypes { get; set; } = default!;
    public DbSet<UserRefreshToken> UserRefreshTokens { get; set; } = default!;
    public DbSet<FavouriteAd> FavouriteAds { get; set; } = default!;


}
