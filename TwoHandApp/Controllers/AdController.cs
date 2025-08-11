using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TwoHandApp.Dto;
using TwoHandApp.Dtos;
using TwoHandApp.Enums;
using TwoHandApp.Helpers;
using TwoHandApp.Models;

namespace TwoHandApp.Controllers;

[Route("api/ad")]
[ApiController]
public class AdController(AppDbContext context, UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpGet("approved")]
    public async Task<IActionResult> GetApprovedAds()
    {
        var approvedAds = await context.Ads
            .Where(ad => ad.Status == AdStatus.Active)
            .OrderByDescending(ad => ad.CreatedAt)
            .ToListAsync();

        return Ok(approvedAds);
    }
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingAds()
    {
        var approvedAds = await context.Ads
            .Where(ad => ad.Status == AdStatus.Pending)
            .OrderByDescending(ad => ad.CreatedAt)
            .ToListAsync();

        return Ok(approvedAds);
    }
    [HttpGet("my-ads")]
    public async Task<IActionResult> GetMyAds()
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return Unauthorized();

        var myAds = await context.Ads
            .Where(ad => ad.UserId == user.Id)
            .Select(x => new
            {
                x.Title,
                x.Price,
                x.CreatedAt
            })
            .OrderByDescending(ad => ad.CreatedAt)
            .ToListAsync();

        return Ok(myAds);
    }

    [Authorize(AuthenticationSchemes = "JwtBearer")]
    [Authorize(Policy = "Permission.Ads_Create")]
    [HttpPost("ads")]

    public async Task<IActionResult> CreateAd([FromBody] CreateAdDto dto)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return Unauthorized();

        var userId = user.Id ?? Guid.Empty.ToString();
        var ad = new Ad
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            Price = dto.Price,
            City = dto.City,
            IsNew = dto.IsNew,
            ProductType = dto.ProductType,
            HasDelivery = dto.HasDelivery,
            CreatedAt = DateTime.UtcNow,
            ViewCount = 0,
            Status = AdStatus.Pending,
            UserId = userId.ToString()
        };

        context.Ads.Add(ad);
        await context.SaveChangesAsync();

        return Ok(new { message = "Elan yaradıldı", ad.Id });
    }
    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await userManager.GetUserAsync(User);

        return userId == null ? null : await userManager.FindByIdAsync(userId);
    }

    [HttpPost("{id}/buy-service")]
    public async Task<IActionResult> BuyService(Guid id, [FromBody] PurchaseServiceDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var ad = await context.Ads.Include(a => a.User).FirstOrDefaultAsync(a => a.Id == id);
        if (ad == null) return NotFound("Ad not found.");
        if (ad.UserId != userId) return Forbid();

        var user = ad.User; // уже загружен при Include

        decimal price;
        if (dto.Service.Equals("premium", StringComparison.OrdinalIgnoreCase))
            price = Prices.PremiumPrice;
        else if (dto.Service.Equals("vip", StringComparison.OrdinalIgnoreCase))
            price = Prices.VipPrice;
        else
            return BadRequest("Unknown service");

        if (user.Balance < price || user.Balance is null)
            return BadRequest(new { message = "Insufficient balance", balance = user.Balance });

        using var tx = await context.Database.BeginTransactionAsync();
        try
        {
            // проверка оптимистической конкуренции (если есть RowVersion)
            // _context.Entry(user).OriginalValues["RowVersion"] = user.RowVersion;

            user.Balance -= price;
            if (dto.Service.Equals("premium", StringComparison.OrdinalIgnoreCase))
            {
                ad.IsPremium = true;
                ad.PremiumUntil = (ad.PremiumUntil ?? DateTime.UtcNow) > DateTime.UtcNow
                    ? ad.PremiumUntil.Value.Add(Prices.PremiumDuration)
                    : DateTime.UtcNow.Add(Prices.PremiumDuration);
            }
            else
            {
                ad.IsVip = true;
                ad.VipUntil = (ad.VipUntil ?? DateTime.UtcNow) > DateTime.UtcNow
                    ? ad.VipUntil.Value.Add(Prices.VipDuration)
                    : DateTime.UtcNow.Add(Prices.VipDuration);
            }

            context.Users.Update(user);
            context.Ads.Update(ad);

            await context.SaveChangesAsync();
            await tx.CommitAsync();

            return Ok(new
            {
                message = "Service purchased",
                balance = user.Balance,
                adId = ad.Id,
                isPremium = ad.IsPremium,
                isVip = ad.IsVip,
                premiumUntil = ad.PremiumUntil,
                vipUntil = ad.VipUntil
            });
        }
        catch (DbUpdateConcurrencyException)
        {
            await tx.RollbackAsync();
            return Conflict("Concurrency error, try again.");
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return StatusCode(500, ex.Message);
        }
    }

}
