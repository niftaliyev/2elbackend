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
        
       
        return Ok();
    }
    [Authorize]
    [HttpGet("my-active-ads")]
    public async Task<IActionResult> GetActiveAds()
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return Unauthorized();

        var now = DateTime.UtcNow;

        var active = await _context.Ads
            .Where(ad => ad.Status == AdStatus.Active && user.Id == ad.UserId)
            .Include(x => x.Images)
            .Select(x => new
            {
                x.Id,
                x.Title,
                x.Description,
                Status = ((AdStatus)x.Status).ToString(),
                x.CreatedAt,
                Images = x.Images.Select(x => x.Url),
                Category = x.Category.Name,
                x.Price,
                x.PhoneNumber,
                x.Email,
                IsVip = x.VipExpiresAt != null && x.VipExpiresAt > now,
                IsPremium = x.PremiumExpiresAt != null && x.PremiumExpiresAt > now,
                IsBoosted = x.BoostedAt != null && x.BoostedAt > DateTime.MinValue,
                x.BoostedAt,
                x.IsNew,
                x.IsDeliverable,
                x.ViewCount,
                x.ExpiresAt,
                City = x.City.Name,
                AdType = x.AdType.Name,
                x.FullName
            })
            .OrderByDescending(ad => ad.IsVip)                           // VIP сверху
            .ThenByDescending(ad => ad.IsPremium)                        // потом Premium
            .ThenByDescending(ad => ad.BoostedAt ?? DateTime.MinValue)   // потом Boosted
            .ThenByDescending(ad => ad.CreatedAt)                        // потом свежие
            .ToListAsync();

        return Ok(active);
    }
    [Authorize]
    [HttpGet("my-inactive-ads")]
    public async Task<IActionResult> GetInActiveAds()
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return Unauthorized();

        var now = DateTime.UtcNow;

        var inactive = await _context.Ads
            .Where(ad => ad.Status == AdStatus.Inactive && user.Id == ad.UserId)
            .Include(x => x.Images)
            .Select(x => new
            {
                x.Id,
                x.Title,
                x.Description,
                Status = ((AdStatus)x.Status).ToString(),
                x.CreatedAt,
                Images = x.Images.Select(x => x.Url),
                Category = x.Category.Name,
                x.Price,
                x.PhoneNumber,
                x.Email,
                IsVip = x.VipExpiresAt != null && x.VipExpiresAt > now,
                IsPremium = x.PremiumExpiresAt != null && x.PremiumExpiresAt > now,
                IsBoosted = x.BoostedAt != null && x.BoostedAt > DateTime.MinValue,
                x.BoostedAt,
                x.IsNew,
                x.IsDeliverable,
                x.ViewCount,
                x.ExpiresAt,
                City = x.City.Name,
                AdType = x.AdType.Name,
                x.FullName
            })
            .OrderBy(ad => ad.CreatedAt)                        // потом свежие
            .ToListAsync();

        return Ok(inactive);
    }
    [Authorize]
    [HttpGet("my-rejected-ads")]
    public async Task<IActionResult> GetRejectedAds()
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return Unauthorized();

        var now = DateTime.UtcNow;

        var rejected = await _context.Ads
            .Where(ad => ad.Status == AdStatus.Rejected && user.Id == ad.UserId)
            .Include(x => x.Images)
            .Select(x => new
            {
                x.Id,
                x.Title,
                x.Description,
                Status = ((AdStatus)x.Status).ToString(),
                x.CreatedAt,
                Images = x.Images.Select(x => x.Url),
                Category = x.Category.Name,
                x.Price,
                x.PhoneNumber,
                x.Email,
                IsVip = x.VipExpiresAt != null && x.VipExpiresAt > now,
                IsPremium = x.PremiumExpiresAt != null && x.PremiumExpiresAt > now,
                IsBoosted = x.BoostedAt != null && x.BoostedAt > DateTime.MinValue,
                x.BoostedAt,
                x.IsNew,
                x.IsDeliverable,
                x.ViewCount,
                x.ExpiresAt,
                City = x.City.Name,
                AdType = x.AdType.Name,
                x.FullName
            })
            .OrderBy(ad => ad.CreatedAt)                        // потом свежие
            .ToListAsync();

        return Ok(rejected);
    }
    
    [Authorize]
    [HttpGet("my-pending-ads")]
    public async Task<IActionResult> GetPendingAds()
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return Unauthorized();

        var now = DateTime.UtcNow;

        var pending = await _context.Ads
            .Where(ad => ad.Status == AdStatus.Pending && user.Id == ad.UserId)
            .Include(x => x.Images)
            .Select(x => new
            {
                x.Id,
                x.Title,
                x.Description,
                Status = ((AdStatus)x.Status).ToString(),
                x.CreatedAt,
                Images = x.Images.Select(x => x.Url),
                Category = x.Category.Name,
                x.Price,
                x.PhoneNumber,
                x.Email,
                IsVip = x.VipExpiresAt != null && x.VipExpiresAt > now,
                IsPremium = x.PremiumExpiresAt != null && x.PremiumExpiresAt > now,
                IsBoosted = x.BoostedAt != null && x.BoostedAt > DateTime.MinValue,
                x.BoostedAt,
                x.IsNew,
                x.IsDeliverable,
                x.ViewCount,
                x.ExpiresAt,
                City = x.City.Name,
                AdType = x.AdType.Name,
                x.FullName
            })
            .OrderBy(ad => ad.CreatedAt)                        // потом свежие
            .ToListAsync();

        return Ok(pending);
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
