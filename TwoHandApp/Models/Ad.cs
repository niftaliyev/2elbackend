using System.Text.Json.Serialization;
using TwoHandApp.Enums;

namespace TwoHandApp.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

public class Ad
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    [JsonPropertyName("name")]
    public string Title { get; set; } = default!;

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = default!;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public bool IsNew { get; set; }

    [JsonPropertyName("isDeliverable")]
    public bool IsDeliverable { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? BoostedAt { get; set; } = null;
    
    public int ViewCount { get; set; }

    public DateTime? PremiumExpiresAt { get; set; } = null;
    public DateTime? VipExpiresAt { get; set; } = null;
    public DateTime? ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(30);

    // FK -> User
    [Required]
    public string UserId { get; set; } = default!;
    public ApplicationUser User { get; set; } = default!;

    // FK -> Category
    [Required]
    public int? CategoryId { get; set; }
    public Category? Category { get; set; } = default!;

    // FK -> City
    [Required]
    public int? CityId { get; set; }
    public City? City { get; set; } = default!;

    // FK -> AdType
    [Required]
    public int? AdTypeId { get; set; }
    public AdType? AdType { get; set; } = default!;

    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = default!;

    [Required]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = default!;

    [Required]
    [MaxLength(150)]
    public string Email { get; set; } = default!;

    public ICollection<AdImage> Images { get; set; } = new List<AdImage>();

    // Enum хранится как int в БД
    [Required]
    public AdStatus Status { get; set; }
    
    public ICollection<FavouriteAd> Favourites { get; set; } = new List<FavouriteAd>();

}