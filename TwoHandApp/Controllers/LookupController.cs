using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoHandApp.Dto;
using TwoHandApp.Enums;
using TwoHandApp.Models;

namespace TwoHandApp.Controllers;

[ApiController]
[Route("api/lookup")]
public class LookupController : ControllerBase
{
    private readonly AppDbContext context;

    public LookupController(AppDbContext context)
    {
        this.context = context;
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await context.Categories
            .Select(c => new { c.Id, c.Name })
            .ToListAsync();
        return Ok(categories);
    }

    [HttpGet("cities")]
    public async Task<IActionResult> GetCities()
    {
        var cities = await context.Cities
            .Select(c => new { c.Id, c.Name })
            .ToListAsync();
        return Ok(cities);
    }
    [HttpGet("prices")]
    public async Task<IActionResult> GetPrices()
    {
        var prices = await context.PackagePrices
            .Select(c => new
            {
                c.Id,
                c.Price,
                c.IntervalDay,
                c.Description,
                c.BoostCount,
                c.IntervalHours,
                PackageType = ((PackageType)c.PackageType).ToString() // ✅ enum как string
                
            })
            .ToListAsync();
        return Ok(prices);
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
    [HttpDelete("prices/{id}")]
    public async Task<IActionResult> DeletePrice(Guid id)
    {
        var price = await context.PackagePrices.FindAsync(id);
        context.PackagePrices.Remove(price);
        await context.SaveChangesAsync();
        return NoContent();
    }
    [HttpGet("packages")]
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
    
    [HttpGet("adtypes")]
    public async Task<IActionResult> GetAdTypes()
    {
        var adTypes = await context.AdTypes
            .Select(at => new { at.Id, at.Name })
            .ToListAsync();
        return Ok(adTypes);
    }
}
