using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoHandApp.Dto;

namespace TwoHandApp.Controllers;

[Route("api/admin")]
[ApiController]
public class AdminController(AppDbContext context) : ControllerBase
{
    [HttpPost("approve-ad")]
    public async Task<IActionResult> Approve([FromQuery] string id)
    {
        if (!Guid.TryParse(id, out var adId))
            return BadRequest("Invalid ad ID format.");

        var ad = await context.Ads.FirstOrDefaultAsync(x => x.Id == adId);

        if (ad == null)
            return NotFound("Ad not found.");

        ad.Status = Enums.AdStatus.Active;

        await context.SaveChangesAsync();

        return Ok(new { message = "Ad approved successfully." });
    }
}
