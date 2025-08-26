using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoHandApp.Dto;
using TwoHandApp.Dtos;
using TwoHandApp.Enums;
using TwoHandApp.Models;

namespace TwoHandApp.Controllers;

[Route("api/admin")]
[ApiController]
public class AdminController(AppDbContext context, UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingAds()
    {
        var approvedAds = await context.Ads
            .Where(ad => ad.Status == AdStatus.Pending)
            .Include(ad => ad.Images)
            .OrderByDescending(ad => ad.CreatedAt)
            .ToListAsync();

        return Ok(approvedAds);
    }
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
    [HttpPost("users/credit")]
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

            // лог операции (опционально)
            // _context.BalanceLogs.Add(new BalanceLog { ... });
            // await _context.SaveChangesAsync();

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
