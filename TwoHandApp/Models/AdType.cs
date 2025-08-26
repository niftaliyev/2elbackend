namespace TwoHandApp.Models;

public class AdType
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    
    // Навигация
    public ICollection<Ad> Ads { get; set; } = new List<Ad>();
}