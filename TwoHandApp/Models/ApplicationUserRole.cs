using Microsoft.AspNetCore.Identity;

namespace TwoHandApp.Models;

public class ApplicationUserRole : IdentityUserRole<string>
{
    public ApplicationUser User { get; set; }
    public ApplicationRole Role { get; set; }
}
