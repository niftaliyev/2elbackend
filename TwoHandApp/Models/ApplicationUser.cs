using Microsoft.AspNetCore.Identity;

namespace TwoHandApp.Models;

public class ApplicationUser : IdentityUser
{
    public Guid? Id { get; set; }
    public string? FullName { get; set; } = default!;
    public string? PhoneNumber { get; set; } = default!;
    public string? UserType { get; set; } = "Şəxsi"; // Və ya "Mağaza"

    public List<Ad> Ads { get; set; } = new();
}
