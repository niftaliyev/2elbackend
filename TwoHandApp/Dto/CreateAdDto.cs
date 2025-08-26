namespace TwoHandApp.Dto;

public class CreateAdDto
{
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
    public bool IsNew { get; set; }
    public bool IsDeliverable { get; set; }

    public int CategoryId { get; set; }
    public int CityId { get; set; }
    public int AdTypeId { get; set; }

    public string FullName { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string Email { get; set; } = default!;

    // Массив файлов
    public List<IFormFile> Images { get; set; } = new();
}
