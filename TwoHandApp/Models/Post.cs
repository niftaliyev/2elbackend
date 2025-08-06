using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TwoHandApp.Models;

public class Post
{
    public int Id { get; set; }

    // 1) Ad title
    [Required]
    [MaxLength(200)]
    public string Title { get; set; }

    // 2) Photos
    //public List<PostPhoto> Photos { get; set; } = new();

    // 3) Save to favorites and report — handled in separate entities (e.g., FavoriteAd, AdReport)

    // 4) Price, poster type, contact number, write message button
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Required]
    [MaxLength(50)]
    public string PosterType { get; set; } // e.g. Individual, Business

    public string ContactNumber { get; set; }

    public bool AllowMessages { get; set; }

    // 5) City, Is new
    [Required]
    public string City { get; set; }

    public bool IsNew { get; set; }

    // 6) Product type, delivery
    [Required]
    public string ProductType { get; set; }

    public bool HasDelivery { get; set; }

    // 7) Description
    [Required]
    public string Description { get; set; }

    // 8) Ad number, Date, Number of views
    [Required]
    [MaxLength(100)]
    public string PostNumber { get; set; } = Guid.NewGuid().ToString();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int ViewCount { get; set; }
    public string? imageUrl { get; set; } = "default.jpg";

    public Guid? UserId { get; set; }
    public ApplicationUser User { get; set; } = default!;
}

