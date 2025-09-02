namespace TwoHandApp.Dto;

public class UpdateAdDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public bool? IsNew { get; set; }
    public bool? IsDeliverable { get; set; }
    public int? CategoryId { get; set; }
    public int? CityId { get; set; }
    public int? AdTypeId { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public List<IFormFile>? Images { get; set; }
}

