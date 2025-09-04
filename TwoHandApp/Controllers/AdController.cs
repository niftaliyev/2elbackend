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
using TwoHandApp.Models.Filters;
using TwoHandApp.Models.Pagination;

namespace TwoHandApp.Controllers;

[Route("api/ad")]
[ApiController]
public class AdController(AppDbContext context, UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpPost("approved-ads")]
    public async Task<ResponsePaginationModel<IEnumerable<dynamic>>> GetApprovedAds(SearchParams<AdFilter> searchParams,CancellationToken cancellationToken)
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
            .ToListAsync(cancellationToken);

        var result = approvedAds.ApplySorting(searchParams.Sort.Select(x => (x.field, x.order)).ToList())
                                                    .Pagination(searchParams.PageNumber, searchParams.PageSize).Select(x => x);


        var pagedList = new PagedList<dynamic>(result,
            result.Count(),
            searchParams.PageNumber,
            searchParams.PageSize);
        

        return ResponsePaginationModel<IEnumerable<dynamic>>.Ok(result,
            pagedList.PaginationData.CurrentPage,
            pagedList.PaginationData.TotalPages,
            pagedList.PaginationData.PageSize,
            pagedList.PaginationData.TotalCount);
    }

[Authorize(AuthenticationSchemes = "JwtBearer")]
[HttpPut("{id}")]
public async Task<IActionResult> UpdateAd(int id, [FromForm] UpdateAdDto dto)
{
    var user = await GetCurrentUserAsync();
    if (user == null)
        return Unauthorized();

    var ad = await context.Ads
        .Include(a => a.Images)
        .FirstOrDefaultAsync(a => a.Id == id);

    if (ad == null)
        return NotFound();

    if (ad.UserId != user.Id)
        return Forbid();

    // Обновляем поля
    ad.Title = string.IsNullOrWhiteSpace(dto.Title) ? ad.Title : dto.Title;
    ad.Description = string.IsNullOrWhiteSpace(dto.Description) ? ad.Description : dto.Description;

    if (dto.Price.HasValue)
        ad.Price = dto.Price.Value;

    if (dto.CategoryId.HasValue)
        ad.CategoryId = dto.CategoryId.Value;
    if (dto.CityId.HasValue)
        ad.CityId = dto.CityId.Value;
    if (dto.AdTypeId.HasValue)
        ad.AdTypeId = dto.AdTypeId.Value;
    
    ad.IsNew = dto.IsNew ?? ad.IsNew;
    ad.IsDeliverable = dto.IsDeliverable ?? ad.IsDeliverable;
    

    ad.FullName = string.IsNullOrWhiteSpace(dto.FullName) ? ad.FullName : dto.FullName;
    ad.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? ad.PhoneNumber : dto.PhoneNumber;
    ad.Email = string.IsNullOrWhiteSpace(dto.Email) ? ad.Email : dto.Email;
    ad.Status = AdStatus.Pending;
    
    // Обновление изображений (если переданы)
    if (dto.Images != null && dto.Images.Count > 0)
    {
        // Удаляем старые изображения
        foreach (var image in ad.Images)
        {
            var filePath = Path.Combine("wwwroot", image.Url.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }
        ad.Images.Clear();

        // Добавляем новые изображения
        foreach (var file in dto.Images)
        {
            if (file.Length > 0)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine("wwwroot/uploads/ads", fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                await using (var stream = new FileStream(filePath, FileMode.Create))
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
    }

    await context.SaveChangesAsync();

    return Ok(new { message = "Elan moderatorlar terefinden yoxlanib yeniden derc olunacaq", ad.Id });
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
