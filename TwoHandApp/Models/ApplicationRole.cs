using Microsoft.AspNetCore.Identity;
using TwoHandApp.Enums;

namespace TwoHandApp.Models;

public class ApplicationRole : IdentityRole<string>
{
    public ICollection<RolePermission> Permissions { get; set; }
}


