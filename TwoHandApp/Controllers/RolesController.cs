using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoHandApp.Enums;
using TwoHandApp.Models;

namespace TwoHandApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin")]
public class RolesController : ControllerBase
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _context;

    public RolesController(RoleManager<ApplicationRole> roleManager,
                           UserManager<ApplicationUser> userManager,
                           AppDbContext context)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _context = context;
    }

    // ✅ Получить все роли
    [HttpGet]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _roleManager.Roles
     .Include(x => x.Permissions)
     .ToListAsync();

        var result = roles.Select(r => new RoleDto
        {
            Id = r.Id,
            Name = r.Name,
            Permissions = r.Permissions.Select(p => p.Permission.ToString()).ToList()
        });

        return Ok(result);
    }

    // ✅ Создать новую роль
    [HttpPost]
    public async Task<IActionResult> CreateRole(string roleName)
    {
        if (await _roleManager.RoleExistsAsync(roleName))
            return BadRequest("Role already exists");

        var role = new ApplicationRole { Name = roleName };
        await _roleManager.CreateAsync(role);
        return Ok(role);
    }

    // ✅ Добавить permission к роли
    [HttpPost("{roleName}/permissions")]
    public async Task<IActionResult> AddPermissionsToRole(string roleName, [FromBody] List<Permission> permissions)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null)
            return NotFound("Role not found");

        foreach (var perm in permissions)
        {
            bool exists = await _context.RolePermissions
                .AnyAsync(x => x.RoleId == role.Id && x.Permission == perm);

            if (!exists)
            {
                _context.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    Permission = perm
                });
            }
        }

        await _context.SaveChangesAsync();
        return Ok("Permissions added");
    }

    // ✅ Назначить роль пользователю
    [HttpPost("assign-role")]
    public async Task<IActionResult> AssignRoleToUser(string email, string roleName)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return NotFound("User not found");

        if (!await _roleManager.RoleExistsAsync(roleName))
            return NotFound("Role not found");

        await _userManager.AddToRoleAsync(user, roleName);
        return Ok("Role assigned");
    }
}

public class RoleDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<string> Permissions { get; set; }
}