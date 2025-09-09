using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoHandApp.Dto;
using TwoHandApp.Enums;
using TwoHandApp.Models;

namespace TwoHandApp.Controllers;

[ApiController]
[Route("api/packages")]
public class PackagesController : ControllerBase
{
    private readonly AppDbContext context;

    public PackagesController(AppDbContext context)
    {
        this.context = context;
    }
    [HttpGet("vip-list")]
    public async Task<IActionResult> Vips()
    {
        var vips = await context.PackagePrices
            .Where(x => x.PackageType == PackageType.Vip)
            .Select(c => new
            {
                c.Id,
                c.Price,
                c.IntervalDay,
                c.Description,
                PackageType = ((PackageType)c.PackageType).ToString() // ✅ enum как string
                
            })
            .ToListAsync();
        return Ok(vips);
    }
    [HttpGet("premium-list")]
    public async Task<IActionResult> Premiums()
    {
        var premiums = await context.PackagePrices
            .Where(x => x.PackageType == PackageType.Premium)
            .Select(c => new
            {
                c.Id,
                c.Price,
                c.IntervalDay,
                c.Description,
                PackageType = ((PackageType)c.PackageType).ToString() // ✅ enum как string
                
            })
            .ToListAsync();
        return Ok(premiums);
    }
    [HttpGet("boosts")]
    public async Task<IActionResult> Boost()
    {
        var premiums = await context.PackagePrices
            .Where(x => x.PackageType == PackageType.Boost)
            .Select(c => new
            {
                c.Id,
                c.Price,
                c.IntervalHours,
                c.Description,
                c.BoostCount,
                PackageType = ((PackageType)c.PackageType).ToString() // ✅ enum как string
                
            })
            .ToListAsync();
        return Ok(premiums);
    }
    
    
    [HttpGet]
    public async Task<IActionResult> GetPackages()
    {
        var packages =  Enum.GetValues(typeof(PackageType))
            .Cast<PackageType>()
            .Select(e => new
            {
                Value = Convert.ToInt32(e),
                Name = e.ToString()
            })
            .ToList();
        return Ok(packages);
    }
    [HttpPost("prices")]
    public async Task<IActionResult> AddPrice([FromBody] PackagePriceDto packagePriceDto)
    {
        var packagePrice = new PackagePrice();
        packagePrice.PackageType = (PackageType)packagePriceDto.PackageType;
        packagePrice.Price = packagePriceDto.Price;
        packagePrice.IntervalDay = packagePriceDto.IntervalDay;
        packagePrice.Description = packagePriceDto.Description;
        packagePrice.IntervalHours = packagePriceDto.IntervalHours;
        packagePrice.BoostCount = packagePriceDto.BoostCount;
        context.PackagePrices.Add(packagePrice);
        await context.SaveChangesAsync();
        return Ok();
    }
    [HttpDelete]
    public async Task<IActionResult> DeletePackage(Guid id)
    {
        var price = await context.PackagePrices.FindAsync(id);
        context.PackagePrices.Remove(price);
        await context.SaveChangesAsync();
        return NoContent();
    }
}