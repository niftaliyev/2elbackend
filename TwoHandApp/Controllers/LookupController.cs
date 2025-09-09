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
    // [HttpGet("prices")]
    // public async Task<IActionResult> GetPrices()
    // {
    //     var prices = await context.PackagePrices
    //         .Select(c => new
    //         {
    //             c.Id,
    //             c.Price,
    //             c.IntervalDay,
    //             c.Description,
    //             c.BoostCount,
    //             c.IntervalHours,
    //             PackageType = ((PackageType)c.PackageType).ToString() // ✅ enum как string
    //             
    //         })
    //         .ToListAsync();
    //     return Ok(prices);
    // }
    
}
