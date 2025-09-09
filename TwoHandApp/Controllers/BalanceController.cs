using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TwoHandApp.Dtos;
using TwoHandApp.Models;

namespace TwoHandApp.Controllers;
[ApiController]
[Route("api/balance")]
public class BalanceController(AppDbContext context, UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpPost("increase")]
    public async Task<IActionResult> GetCategories(IncreaseBalanceDto increaseBalanceDto)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return Unauthorized();


        if (!(increaseBalanceDto?.Image?.Length > 0)) throw new ArgumentException("bad request");
        
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(increaseBalanceDto.Image.FileName)}";
            var filePath = Path.Combine("wwwroot/uploads/invoices", fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await increaseBalanceDto.Image.CopyToAsync(stream);
            }

            var increaseBalance = new IncreaseBalance
            {
                Image = $"/uploads/ads/{fileName}",
                UserId = user.Id,
                Amount = increaseBalanceDto.Amount,
                Name = user?.UserName   
            };
            context.IncreaseBalances.Add(increaseBalance);
            await context.SaveChangesAsync();
            return Ok();
    }
    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await userManager.GetUserAsync(User);

        return userId == null ? null : await userManager.FindByIdAsync(userId);
    }
}