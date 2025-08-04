using TwoHandApp.Enums;

namespace TwoHandApp.Models;

public class Ad
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }

    public DateTime CreatedAt { get; set; }
    public int ViewCount { get; set; }
    public string City { get; set; } = default!;

    public bool IsPremium { get; set; }
    public bool IsVip { get; set; }
    public bool IsStoreAd { get; set; }

    public bool IsNew { get; set; } // Yeni? Bəli/Xeyr
    public string ProductType { get; set; } = default!; // Məsələn: Telefon, Avto, Geyim
    public bool HasDelivery { get; set; }

    public AdStatus Status { get; set; }

    // Foreign Keys
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = default!;

    public List<AdPhoto> Photos { get; set; } = new();
}
