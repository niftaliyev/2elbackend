namespace TwoHandApp.Models;

public class FavouriteAd
{
    public Guid Id { get; set; }

    // Кто добавил в избранное
    public string UserId { get; set; } = default!;
    public ApplicationUser User { get; set; } = default!;

    // Какое объявление добавлено
    public int AdId { get; set; }
    public Ad Ad { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
