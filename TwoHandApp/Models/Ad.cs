using System.Text.Json.Serialization;
using TwoHandApp.Enums;

namespace TwoHandApp.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

public class Ad
{
    [Key]
    public Guid Id { get; set; }

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

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int ViewCount { get; set; }

    public bool IsPremium { get; set; }
    public bool IsVip { get; set; }
    public DateTime? PremiumUntil { get; set; }
    public DateTime? VipUntil { get; set; }

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
}