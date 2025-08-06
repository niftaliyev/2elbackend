namespace TwoHandApp.Dto;

public class CreateAdDto
{
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
    public string City { get; set; } = default!;
    public bool IsNew { get; set; }
    public string ProductType { get; set; } = default!;
    public bool HasDelivery { get; set; }
}
