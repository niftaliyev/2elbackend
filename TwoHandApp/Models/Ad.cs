using TwoHandApp.Enums;

namespace TwoHandApp.Models;

public class Ad
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
    public string City { get; set; } = default!;
    public bool IsNew { get; set; }
    public string ProductType { get; set; } = default!;
    public bool HasDelivery { get; set; }

    public DateTime CreatedAt { get; set; }
    public int ViewCount { get; set; }

    public bool IsPremium { get; set; }
    public bool IsVip { get; set; }

    // когда куплено — дата
    public DateTime? PremiumUntil { get; set; }
    public DateTime? VipUntil { get; set; }

    // Foreign Key
    public string UserId { get; set; } = default!;
    public AdStatus Status { get; set; }

    // Навигационное свойство
    public ApplicationUser User { get; set; } = default!;
}

