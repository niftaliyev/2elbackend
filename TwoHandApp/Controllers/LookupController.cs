using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

    [HttpGet("adtypes")]
    public async Task<IActionResult> GetAdTypes()
    {
        var adTypes = await context.AdTypes
            .Select(at => new { at.Id, at.Name })
            .ToListAsync();
        return Ok(adTypes);
    }
}
