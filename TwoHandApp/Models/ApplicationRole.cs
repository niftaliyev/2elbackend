using Microsoft.AspNetCore.Identity;
using TwoHandApp.Enums;

namespace TwoHandApp.Models;

public class ApplicationRole : IdentityRole
{
    public ICollection<RolePermission> Permissions { get; set; }
}


public class RolePermission
{
    public int Id { get; set; }
    public string RoleId { get; set; }
    public Permission Permission { get; set; }

    public ApplicationRole Role { get; set; }
}