namespace TwoHandApp.Dto;

public class UpdateAdDto
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public bool IsNew { get; set; }
    public bool IsDeliverable { get; set; }
    public int CategoryId { get; set; }
    public int CityId { get; set; }
    public int AdTypeId { get; set; }
    public string FullName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Email { get; set; } = null!;
    public List<IFormFile>? Images { get; set; } // optional
}
