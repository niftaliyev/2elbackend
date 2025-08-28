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
    [HttpGet("approved-ads")]
    public async Task<IActionResult> GetApprovedAds()
    {
        var now = DateTime.UtcNow;

        var approvedAds = await context.Ads
            .Where(ad => ad.Status == AdStatus.Active && ad.ExpiresAt > now)
            .Include(x => x.Images)
            .Select(x => new
            {
                x.Id,
                x.Title,
                x.Description,
                x.Status,
                x.CreatedAt,
                x.Images,
                x.Category,
                x.Price,
                x.PhoneNumber,
                x.Email,
                IsVip = x.VipExpiresAt != null && x.VipExpiresAt > now,
                IsPremium = x.PremiumExpiresAt != null && x.PremiumExpiresAt > now,
                IsBoosted = x.BoostedAt != null && x.BoostedAt > DateTime.MinValue,
                x.BoostedAt
            })
            .OrderByDescending(ad => ad.IsVip)                           // VIP сверху
            .ThenByDescending(ad => ad.IsPremium)                        // потом Premium
            .ThenByDescending(ad => ad.BoostedAt ?? DateTime.MinValue)   // потом Boosted
            .ThenByDescending(ad => ad.CreatedAt)                        // потом свежие
            .ToListAsync();

        return Ok(approvedAds);
    }



    [Authorize(AuthenticationSchemes = "JwtBearer")]
    [Authorize(Policy = "Permission.Ads_Create")]
    [HttpPost("ads")]
    public async Task<IActionResult> CreateAd([FromForm] CreateAdDto dto)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return Unauthorized();

        var ad = new Ad
        {
            Title = dto.Title,
            Description = dto.Description,
            Price = dto.Price,
            IsNew = dto.IsNew,
            IsDeliverable = dto.IsDeliverable,
            CreatedAt = DateTime.UtcNow,
            ViewCount = 0,
            Status = AdStatus.Pending,
            UserId = user.Id,
            CategoryId = dto.CategoryId,
            CityId = dto.CityId,
            AdTypeId = dto.AdTypeId,
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            BoostedAt = null
        };

        // Сохраняем файлы
        foreach (var file in dto.Images)
        {
            if (file.Length > 0)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine("wwwroot/uploads/ads", fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                ad.Images.Add(new AdImage
                {
                    Id = Guid.NewGuid(),
                    Url = $"/uploads/ads/{fileName}",
                    AdId = ad.Id
                });
            }
        }

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
}
