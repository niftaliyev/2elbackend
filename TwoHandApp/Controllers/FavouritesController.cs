using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoHandApp.Models;
using TwoHandApp.Models.Filters;
using TwoHandApp.Models.Pagination;

namespace TwoHandApp.Controllers;

[ApiController]
[Route("api/favourites")]
[Authorize(AuthenticationSchemes = "JwtBearer")]
public class FavouritesController : ControllerBase
{
    private readonly AppDbContext context;
    private readonly UserManager<ApplicationUser> userManager;

    public FavouritesController(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        this.context = context;
        this.userManager = userManager;
    }

    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return userId == null ? null : await userManager.FindByIdAsync(userId);
    }

    // Добавить объявление в избранное
    [HttpGet("add")]
    public async Task<IActionResult> AddToFavourites(int adId)
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return Unauthorized();

        if (await context.FavouriteAds.AnyAsync(f => f.UserId == user.Id && f.AdId == adId))
            return BadRequest("Ad already in favourites");

        var favourite = new FavouriteAd
        {
            UserId = user.Id,
            AdId = adId
        };

        context.FavouriteAds.Add(favourite);
        await context.SaveChangesAsync();
        return Ok();
    }

    // Удалить из избранного
    [HttpDelete]
    public async Task<IActionResult> RemoveFromFavourites(int adId)
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return Unauthorized();

        var favourite = await context.FavouriteAds
            .FirstOrDefaultAsync(f => f.UserId == user.Id && f.AdId == adId);

        if (favourite == null) return NotFound();

        context.FavouriteAds.Remove(favourite);
        await context.SaveChangesAsync();
        return Ok();
    }

    // Получить все избранные объявления пользователя
    [HttpPost]
    public async Task<IActionResult> GetFavourites(SearchParams<AdFilter> searchParams,CancellationToken cancellationToken)
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return Unauthorized();

        var favourites = await context.FavouriteAds
            .Where(f => f.UserId == user.Id)
            .Select(f => new
            {
                f.AdId,
                f.Ad.Title,
                f.Ad.Description,
                f.Ad.Price,
                f.Ad.IsNew,
                f.Ad.IsDeliverable,
                f.Ad.Images
            })
            .ToListAsync();

        var result = favourites.ApplySorting(searchParams.Sort.Select(x => (x.field, x.order)).ToList())
            .Pagination(searchParams.PageNumber, searchParams.PageSize).Select(x => x);
        return Ok(result);
    }
}
