namespace TwoHandApp.Models;

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
    public string? Image { get; set; } = "res.jpg";
}
