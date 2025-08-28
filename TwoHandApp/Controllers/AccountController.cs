using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TwoHandApp.Dto;
using TwoHandApp.Dtos;
using TwoHandApp.Enums;
using TwoHandApp.Models;
using TwoHandApp.Regexs;
using TwoHandApp.Helpers;

namespace TwoHandApp.Controllers;

[ApiController]
[Route("api/account")]
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
    // var claimValues = claims
    //     .GroupBy(c => c.Type)
    //     .ToDictionary(g => g.Key, g => g.Select(c => c.Value).ToList());

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
            id = user.Id,
            fullName = user.FullName,
            email = user.Email,
            phoneNumber = user.PhoneNumber,
            balance = user.Balance,
            roles = roles.ToList()
        });
    }
    
    [Authorize]
    [HttpPost("{id}/buy-service")]
    public async Task<IActionResult> BuyService(int? id, [FromBody] PurchaseServiceDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var ad = await _context.Ads.Include(a => a.User).FirstOrDefaultAsync(a => a.Id == id);
        var packagePrice = await _context.PackagePrices.FirstOrDefaultAsync(a => a.Id == Guid.Parse(dto.priceid));
        if (ad == null) return NotFound("Ad not found.");
        if (ad.UserId != userId) return Forbid();

        var user = ad.User; // уже загружен при Include
        if (user.Balance < packagePrice.Price || user.Balance is null)
            return BadRequest(new { message = "Insufficient balance", balance = user.Balance });
        
        using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            switch ((PackageType)packagePrice.PackageType)
            {
                case PackageType.Vip:
                    user.Balance -= packagePrice.Price;
                    ad.VipExpiresAt = DateTime.UtcNow.AddDays(packagePrice.IntervalDay ?? 1);
                    _context.Users.Update(user);
                    _context.Ads.Update(ad);
                
                    await _context.SaveChangesAsync();
                    await tx.CommitAsync();
                    break;
                case PackageType.Premium:
                    user.Balance -= packagePrice.Price;
                    ad.PremiumExpiresAt = DateTime.UtcNow.AddDays(packagePrice.IntervalDay ?? 1);
                    _context.Users.Update(user);
                    _context.Ads.Update(ad);
                
                    await _context.SaveChangesAsync();
                    await tx.CommitAsync();
                    break;
                case PackageType.Boost:
                    UserAdPackage userAdPackage= new UserAdPackage();
                    userAdPackage.Id = Guid.NewGuid();
                    userAdPackage.AdId = ad.Id;
                    userAdPackage.PackagePriceId = packagePrice.Id;
                    userAdPackage.StartDate = DateTime.UtcNow;
                    userAdPackage.BoostsRemaining = packagePrice.BoostCount;
                    userAdPackage.LastBoostedAt = DateTime.UtcNow;
                    userAdPackage.Type = PackageType.Boost;
                    userAdPackage.EndDate = DateTime.UtcNow.AddHours(packagePrice.IntervalHours ?? 1);
                    
                    
                    ad.BoostedAt = DateTime.UtcNow;
                    _context.Ads.Update(ad);
                    _context.Users.Update(user);
                    _context.UserAdPackages.Add(userAdPackage);
                    await _context.SaveChangesAsync();
                    await tx.CommitAsync();
                    break;
                
    
            }
        }
        catch (Exception e)
        {
            await tx.RollbackAsync();
            throw e;
        }
        
        // decimal price;
        // if (dto.Service.Equals("premium", StringComparison.OrdinalIgnoreCase))
        //     price = Prices.PremiumPrice;
        // else if (dto.Service.Equals("vip", StringComparison.OrdinalIgnoreCase))
        //     price = Prices.VipPrice;
        // else
        //     return BadRequest("Unknown service");
        //
        // if (user.Balance < price || user.Balance is null)
        //     return BadRequest(new { message = "Insufficient balance", balance = user.Balance });
        //
        // using var tx = await _context.Database.BeginTransactionAsync();
        // try
        // {
        //     // проверка оптимистической конкуренции (если есть RowVersion)
        //     // _context.Entry(user).OriginalValues["RowVersion"] = user.RowVersion;
        //
        //     user.Balance -= price;
        //     if (dto.Service.Equals("premium", StringComparison.OrdinalIgnoreCase))
        //     {
        //         ad.PremiumExpiresAt = DateTime.UtcNow;
        //     }
        //     else
        //     {
        //         ad.VipExpiresAt = DateTime.UtcNow;
        //     }
        //
        //     _context.Users.Update(user);
        //     _context.Ads.Update(ad);
        //
        //     await _context.SaveChangesAsync();
        //     await tx.CommitAsync();
        //
        //     var now = DateTime.UtcNow;
        //
        //     return Ok(new
        //     {
        //         message = "Service purchased",
        //         balance = user.Balance,
        //         adId = ad.Id,
        //         isPremium = ad.PremiumExpiresAt != null && ad.PremiumExpiresAt > now,
        //         isVip = ad.VipExpiresAt != null && ad.VipExpiresAt > now
        //     });
        // }
        // catch (DbUpdateConcurrencyException)
        // {
        //     await tx.RollbackAsync();
        //     return Conflict("Concurrency error, try again.");
        // }
        // catch (Exception ex)
        // {
        //     await tx.RollbackAsync();
        //     return StatusCode(500, ex.Message);
        // }
        return Ok();
    }
    [Authorize]
    [HttpGet("active-ads")]
    public async Task<IActionResult> GetActiveAds()
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return Unauthorized();

        var active = await _context.Ads
            .Where(ad => ad.UserId == user.Id && ad.Status == AdStatus.Active)
            .Include(ad => ad.Images)
            .Include(ad => ad.User) // <- включаем User
            .Select(f => new AdDto
            {
                Name = f.FullName,
                Description = f.Description,
                Price = f.Price,
                IsNew = f.IsNew,
                IsDeliverable = f.IsDeliverable,
                CreatedAt = f.CreatedAt,
                UserId = f.UserId,
                UserFullName = f.User.FullName, // теперь безопасно
                Images = f.Images.Select(i => new AdImageDto
                {
                    Id = i.Id,
                    Url = i.Url
                }).ToList()
            })
            .OrderByDescending(ad => ad.CreatedAt)
            .ToListAsync();



        return Ok(active);
    }
    [Authorize]
    [HttpGet("inactive-ads")]
    public async Task<IActionResult> GetInActiveAds()
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return Unauthorized();

        var inActive = await _context.Ads
            .Where(ad => ad.UserId == user.Id && ad.Status == AdStatus.Inactive)
            .Include(ad => ad.Images)
            .Include(ad => ad.User) // <- включаем User
            .Select(f => new AdDto
            {
                Name = f.FullName,
                Description = f.Description,
                Price = f.Price,
                IsNew = f.IsNew,
                IsDeliverable = f.IsDeliverable,
                CreatedAt = f.CreatedAt,
                UserId = f.UserId,
                UserFullName = f.User.FullName, // теперь безопасно
                Images = f.Images.Select(i => new AdImageDto
                {
                    Id = i.Id,
                    Url = i.Url
                }).ToList()
            })
            .OrderByDescending(ad => ad.CreatedAt)
            .ToListAsync();



        return Ok(inActive);
    }
    [Authorize]
    [HttpGet("rejected-ads")]
    public async Task<IActionResult> GetRejectedAds()
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return Unauthorized();

        var rejected = await _context.Ads
            .Where(ad => ad.UserId == user.Id && ad.Status == AdStatus.Rejected)
            .Include(ad => ad.Images)
            .Include(ad => ad.User) // <- включаем User
            .Select(f => new AdDto
            {
                Name = f.FullName,
                Description = f.Description,
                Price = f.Price,
                IsNew = f.IsNew,
                IsDeliverable = f.IsDeliverable,
                CreatedAt = f.CreatedAt,
                UserId = f.UserId,
                UserFullName = f.User.FullName, // теперь безопасно
                Images = f.Images.Select(i => new AdImageDto
                {
                    Id = i.Id,
                    Url = i.Url
                }).ToList()
            })
            .OrderByDescending(ad => ad.CreatedAt)
            .ToListAsync();



        return Ok(rejected);
    }
    [Authorize]
    [HttpGet("pending-ads")]
    public async Task<IActionResult> GetPendingAds()
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return Unauthorized();

        var rejected = await _context.Ads
            .Where(ad => ad.UserId == user.Id && ad.Status == AdStatus.Pending)
            .Include(ad => ad.Images)
            .Include(ad => ad.User) // <- включаем User
            .Select(f => new AdDto
            {
                Name = f.FullName,
                Description = f.Description,
                Price = f.Price,
                IsNew = f.IsNew,
                IsDeliverable = f.IsDeliverable,
                CreatedAt = f.CreatedAt,
                UserId = f.UserId,
                UserFullName = f.User.FullName, // теперь безопасно
                Images = f.Images.Select(i => new AdImageDto
                {
                    Id = i.Id,
                    Url = i.Url
                }).ToList()
            })
            .OrderByDescending(ad => ad.CreatedAt)
            .ToListAsync();


        return Ok(rejected);
    }
    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.GetUserAsync(User);

        return userId == null ? null : await _userManager.FindByIdAsync(userId);
    }
}

public class AssignRoleRequest
{
    public string RoleName { get; set; }
}
