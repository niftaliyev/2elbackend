namespace TwoHandApp.Dto;

public class AdDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
    public bool IsNew { get; set; }
    public bool IsDeliverable { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UserId { get; set; }
    public string? UserFullName { get; set; }
    public List<AdImageDto> Images { get; set; } = new();
}