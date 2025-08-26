namespace TwoHandApp.Models;

public class City
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    
    // Навигация
    public ICollection<Ad> Ads { get; set; } = new List<Ad>();
}