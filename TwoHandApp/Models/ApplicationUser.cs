using Microsoft.AspNetCore.Identity;

namespace TwoHandApp.Models;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; } = default!;
    public string? PhoneNumber { get; set; } = default!;
    public string? UserType { get; set; } = "Şəxsi"; // Və ya "Mağaza"

    public ICollection<Ad> Ads { get; set; } = new List<Ad>();

    public ICollection<IdentityUserRole<string>> UserRoles { get; set; } = new List<IdentityUserRole<string>>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

}
