using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TwoHandApp.Dto;
using TwoHandApp.Models;

namespace TwoHandApp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AdController(AppDbContext context, UserManager<ApplicationUser> userManager) : ControllerBase
{
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

}
