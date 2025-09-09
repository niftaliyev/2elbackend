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

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IConfiguration _config;
    private readonly AppDbContext _context;

    public AuthController(
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
        if (!ValidEmail.IsValidEmail(model.Email))
            throw new Exception("Incorrect email");

        var user = new ApplicationUser
        {
            FullName = model.FullName,
            UserName = model.Email,
            Balance = 0,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            EmailConfirmed = true, // Устанавливаем EmailConfirmed в true для упрощения
            PhoneNumberConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);



        if (!await _roleManager.RoleExistsAsync("User"))
            await _roleManager.CreateAsync(new ApplicationRole { Name = "User" });

        await _userManager.AddToRoleAsync(user, "User");

        return Ok("Пользователь зарегистрирован");
    }
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        if (Request.Cookies.TryGetValue("refresh_token", out var refreshToken))
        {
            var dbToken = await _context.UserRefreshTokens
                .FirstOrDefaultAsync(t => t.Token == refreshToken);

            if (dbToken != null)
            {
                dbToken.IsRevoked = true; // или _context.UserRefreshTokens.Remove(dbToken);
                await _context.SaveChangesAsync();
            }
        }

        Response.Cookies.Delete("access_token");
        Response.Cookies.Delete("refresh_token");

        return Ok(new { message = "Logged out" });
    }


    
[HttpPost("refresh")]
public async Task<IActionResult> Refresh()
{
    if (!Request.Cookies.TryGetValue("refresh_token", out var refreshToken))
        return Unauthorized(new { message = "No refresh token" });

    if (string.IsNullOrEmpty(refreshToken))
        return Unauthorized(new { message = "Invalid refresh token" });

    // Проверяем refresh_token в базе
    var dbToken = await _context.UserRefreshTokens
        .FirstOrDefaultAsync(t => t.Token == refreshToken && !t.IsRevoked);

    if (dbToken == null || dbToken.ExpiresAt < DateTime.UtcNow)
        return Unauthorized(new { message = "Invalid or expired refresh token" });

    // достаём access_token (он может быть истёкший, но claims там есть)
    if (!Request.Cookies.TryGetValue("access_token", out var oldAccess))
        return Unauthorized(new { message = "No access token" });

    var handler = new JwtSecurityTokenHandler();
    JwtSecurityToken? oldJwt;
    try
    {
        oldJwt = handler.ReadJwtToken(oldAccess);
    }
    catch
    {
        return Unauthorized(new { message = "Invalid access token" });
    }

    var userId = oldJwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
    if (userId == null) return Unauthorized();

    var user = await _userManager.FindByIdAsync(userId);
    if (user == null) return Unauthorized();

    var roles = await _userManager.GetRolesAsync(user);

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim("fullName", user.FullName ?? ""),
        new Claim("phoneNumber", user.PhoneNumber ?? ""),
        new Claim("userType", user.UserType ?? ""),
        new Claim("Balance", user.Balance.ToString())
    };

    var publicClaims = new Dictionary<string, object>
    {
        ["nameIdentifier"] = user.Id.ToString(),
        ["fullName"] = user.FullName ?? "",
        ["phoneNumber"] = user.PhoneNumber ?? "",
        ["balance"] = user.Balance.ToString(),
        ["email"] = user.Email ?? "",
        ["roles"] = roles.ToList()
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

    // Генерация нового access_token
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
    var token = new JwtSecurityToken(
        issuer: _config["Jwt:Issuer"],
        audience: _config["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddHours(2),
        signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
    );
    var newAccessToken = new JwtSecurityTokenHandler().WriteToken(token);

    // Ротация refresh_token: помечаем старый отозванным и создаём новый
    dbToken.IsRevoked = true;
    var newRefreshToken = Guid.NewGuid().ToString("N");
    _context.UserRefreshTokens.Add(new UserRefreshToken
    {
        UserId = user.Id,
        Token = newRefreshToken,
        ExpiresAt = DateTime.UtcNow.AddDays(7)
    });
    await _context.SaveChangesAsync();

    Response.Cookies.Append("access_token", newAccessToken, new CookieOptions
    {
        HttpOnly = true,
        Secure = false,
        SameSite = SameSiteMode.Lax,
        Expires = DateTimeOffset.UtcNow.AddHours(2),
        Path = "/"
    });

    Response.Cookies.Append("refresh_token", newRefreshToken, new CookieOptions
    {
        HttpOnly = true,
        Secure = false,
        SameSite = SameSiteMode.Lax,
        Expires = DateTimeOffset.UtcNow.AddDays(7),
        Path = "/"
    });

    return Ok(new
    {
        message = "Token refreshed",
        info = publicClaims
    });
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
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim("fullName", user.FullName ?? ""),
        new Claim("phoneNumber", user.PhoneNumber ?? ""),
        new Claim("userType", user.UserType ?? ""),
        new Claim("Balance", user.Balance.ToString())
    };
    var publicClaims = new Dictionary<string, object>
    {
        ["nameIdentifier"] = user.Id.ToString(),
        ["fullName"] = user.FullName ?? "",
        ["phoneNumber"] = user.PhoneNumber ?? "",
        ["balance"] = user.Balance.ToString(),
        ["email"] = user.Email ?? "",
        ["roles"] = roles.ToList()
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

    // Генерация JWT
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
    var token = new JwtSecurityToken(
        issuer: _config["Jwt:Issuer"],
        audience: _config["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddHours(2),
        signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
    );

    var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

    // Ставим JWT в HttpOnly cookie
    Response.Cookies.Append("access_token", jwtToken, new CookieOptions
    {
        HttpOnly = true,
        Secure = false,             // временно отключаем для HTTP
        SameSite = SameSiteMode.Lax,
        Expires = DateTimeOffset.UtcNow.AddHours(2),
        Path ="/"
    });
    
    // Генерация refresh_token (просто GUID, можно хранить в БД если нужно инвалидировать)
    var refreshToken = Guid.NewGuid().ToString("N");
    _context.UserRefreshTokens.Add(new UserRefreshToken
    {
        UserId = user.Id,
        Token = refreshToken,
        ExpiresAt = DateTime.UtcNow.AddDays(7)
    });
    await _context.SaveChangesAsync();
    Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
    {
        HttpOnly = true,
        Secure = false,  // пока без https
        SameSite = SameSiteMode.Lax,
        Expires = DateTimeOffset.UtcNow.AddDays(7),
        Path = "/"
    });

    return Ok(new
    {
        message = "Login successful",
        info = publicClaims
    });
    
}
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        // достаем userId из клейма
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        // достаем роли
        var roles = await _userManager.GetRolesAsync(user);

        // возвращаем инфу
        return Ok(new
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Balance = user.Balance,
            Roles = roles.ToList()
        });
    }
}