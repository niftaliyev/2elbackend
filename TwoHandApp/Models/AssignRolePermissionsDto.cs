namespace TwoHandApp.Models;

public class AssignRolePermissionsDto
{
    public int RoleId { get; set; }
    public List<int> PermissionIds { get; set; } = [];
}
