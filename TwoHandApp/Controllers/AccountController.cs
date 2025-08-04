using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TwoHandApp.Models;
using TwoHandApp.Regexs;

namespace TwoHandApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IConfiguration _config;
    private readonly AppDbContext _context;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IConfiguration config,
        AppDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _config = config;
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if(!ValidEmail.IsValidEmail(model.Email))
            throw new ArgumentException("Incorrect email");

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);



        //if (!await _roleManager.RoleExistsAsync(model.Role))
        //    await _roleManager.CreateAsync(new ApplicationRole { Name = model.Role });

        //await _userManager.AddToRoleAsync(user, model.Role);

        return Ok("Пользователь зарегистрирован");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            return Unauthorized();

        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.NameIdentifier, user.Id)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));

            var roleEntity = await _roleManager.FindByNameAsync(role);
            if (roleEntity != null)
            {
                var permissions = _context.RolePermissions
                    .Where(p => p.RoleId == roleEntity.Id)
                    .Select(p => p.Permission.ToString())
                    .ToList();

                foreach (var perm in permissions)
                {
                    claims.Add(new Claim("permission", perm));
                }
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("users/{userId}/assign-role")]
    public async Task<IActionResult> AssignRole(string userId, [FromBody] string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound("User not found");

        if (!await _roleManager.RoleExistsAsync(roleName))
            await _roleManager.CreateAsync(new ApplicationRole { Name = roleName });

        await _userManager.AddToRoleAsync(user, roleName);
        return Ok("Роль назначена");
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("roles/{roleName}/add-permission")]
    public async Task<IActionResult> AddPermissionToRole(string roleName, [FromBody] Permission permission)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null) return NotFound("Роль не найдена");

        var exists = await _context.RolePermissions
            .AnyAsync(rp => rp.RoleId == role.Id && rp.Permission == permission);

        if (!exists)
        {
            _context.RolePermissions.Add(new RolePermission
            {
                RoleId = role.Id,
                Permission = permission
            });
            await _context.SaveChangesAsync();
        }

        return Ok($"Permission {permission} добавлен роли {roleName}");
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("permissions")]
    public IActionResult GetPermissions()
    {
        var permissions = Enum.GetValues(typeof(Permission))
            .Cast<Permission>()
            .Select(p => p.ToString())
            .ToList();

        return Ok(permissions);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin-only")]
    public IActionResult AdminOnly()
    {
        return Ok("Только для роли Admin");
    }

    [Authorize(Policy = "Permission.Users_View")]
    [HttpGet("permission-check")]
    public IActionResult CheckPermission()
    {
        return Ok("У вас есть разрешение Users_View");
    }
}