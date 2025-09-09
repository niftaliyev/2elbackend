using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoHandApp.Dtos;
using TwoHandApp.Enums;
using TwoHandApp.Models;
using TwoHandApp.Models.Filters;
using TwoHandApp.Models.Pagination;

namespace TwoHandApp.Controllers;

[Route("api/admin")]
[ApiController]
public class AdminController(AppDbContext context, UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpPost("pending")]
    public async Task<IActionResult> GetPendingAds(SearchParams<AdFilter> searchParams,CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var pendingAds = await context.Ads
            .Where(ad => ad.Status == AdStatus.Pending)
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
            })  // потом Boosted
            .OrderBy(ad => ad.CreatedAt)                        // потом свежие
            .ToListAsync(cancellationToken);

        var result = pendingAds.ApplySorting(searchParams.Sort.Select(x => (x.field, x.order)).ToList())
            .Pagination(searchParams.PageNumber, searchParams.PageSize).Select(x => x);
        return Ok(result);
    }
    [HttpPost("approve-ad")]
    public async Task<IActionResult> Approve([FromQuery] int? id)
    {
        if (id == null || id < 1)
            return BadRequest("Invalid ad ID format.");

        var ad = await context.Ads.FirstOrDefaultAsync(x => x.Id == id);

        if (ad == null)
            return NotFound("Ad not found.");

        ad.Status = Enums.AdStatus.Active;

        await context.SaveChangesAsync();

        return Ok(new { message = "Ad approved successfully." });

    }
    [HttpPost("reject-ad")]
    public async Task<IActionResult> Reject([FromQuery] int? id)
    {
        if (id == null || id < 1)
            return BadRequest("Invalid ad ID format.");

        var ad = await context.Ads.FirstOrDefaultAsync(x => x.Id == id);

        if (ad == null)
            return NotFound("Ad not found.");

        ad.Status = Enums.AdStatus.Rejected;

        await context.SaveChangesAsync();

        return Ok(new { message = "Ad rejected successfully." });

    }

    [HttpGet("boosted-ads")]
    public async Task<IActionResult> BoostedAds(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var boosts = await context.UserAdPackages
            .Include(b => b.Ad)
            .Include(b => b.PackagePrice)
            .Where(b => b.BoostsRemaining > 0 && b.EndDate > now)
            .Where(b => b.LastBoostedAt <= now.AddHours(b.PackagePrice.IntervalHours ?? 1))
            .ToListAsync(cancellationToken);
        return Ok(boosts);
    }
    
    [HttpGet("pending-balance-increase")]
    public async Task<IActionResult> GetPendingBalanceIncrease()
    {
        var pendingUsers = context.IncreaseBalances.Select(x => new IncreaseBalanceResponseDto
        {
            amount = x.Amount,
            userId = x.UserId,
            image = x.Image,
            userName = x.Name
        });
        return Ok(pendingUsers);
    }

    [HttpPost("increase-balance")]
    public async Task<IActionResult> CreditUser([FromBody] CreditUserDto dto)
    {
        if (dto.Amount <= 0) return BadRequest("Amount must be positive.");

        var user = await userManager.FindByIdAsync(dto.UserId);
        if (user == null) return NotFound();

        // транзакция — опционально
        using var tx = await context.Database.BeginTransactionAsync();
        try
        {
            user.Balance += dto.Amount;
            context.Users.Update(user);
            await context.SaveChangesAsync();
            
            await tx.CommitAsync();
            return Ok(new { message = "Balance credited", balance = user.Balance });
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return StatusCode(500, ex.Message);
        }
    }
}
