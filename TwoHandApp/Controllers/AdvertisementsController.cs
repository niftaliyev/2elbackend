using Microsoft.AspNetCore.Mvc;
using TwoHandApp.Models;
using System.IO;

namespace TwoHandApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdvertisementsController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly AppDbContext context;

    public AdvertisementsController(IWebHostEnvironment environment, AppDbContext context)
    {
        _environment = environment;
        this.context = context;
    }
    [HttpGet("id/{id}")]
    public ActionResult<IEnumerable<Advertisement>> GetAdvertisement(int id)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var advertisements = context.Posts.FirstOrDefault(x => x.Id == id);

        if (advertisements == null)
        {
            return NotFound(); // Return 404 if advertisement is not found
        }

        var result =  new
        {
            advertisements.Id,
            advertisements.Title,
            advertisements.Description,
            advertisements.Price,
            //advertisements.CreatedDate,
            //advertisements.IsPro,
            //advertisements.IsVip,
            //advertisements.ProExpiryDate,
            //advertisements.VipExpiryDate,
            //ImageUrl = $"{baseUrl}/{advertisements.Image}"
        };

        return Ok(result);
    }
    [HttpGet("my-advertisements")]
    public ActionResult<IEnumerable<Advertisement>> GetMyAdvertisements()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var advertisements = context.Advertisements.ToList().Take(4);

        var result = advertisements.Select(ad => new
        {
            ad.Id,
            ad.Title,
            ad.Description,
            ad.Price,
            ad.CreatedDate,
            ad.IsPro,
            ad.IsVip,
            ad.ProExpiryDate,
            ad.VipExpiryDate,
            ImageUrl = $"{baseUrl}/{ad.Image}"
        });

        return Ok(result);
    }
    [HttpGet]
    public ActionResult<IEnumerable<Advertisement>> All()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        //var advertisements = context.Advertisements.ToList();
        var advertisements2 = context.Posts.ToList();

        var result = advertisements2.Select(ad => new
        {
            ad.Id,
            ad.Title,
            ad.Description,
            ad.Price,
            ad.CreatedAt,
            imageUrl = ad.imageUrl != null && (ad.imageUrl.StartsWith("http://") || ad.imageUrl.StartsWith("https://")) ? ad.imageUrl : $"{baseUrl}/{ad.imageUrl}"
            //ad.IsPro,
            //ad.IsVip,
            //ad.ProExpiryDate,
            //ad.VipExpiryDate,
            //ImageUrl = $"{baseUrl}/{ad.Image}"
        });

        return Ok(result);
    }

    //[HttpGet("{imageName}")]
    //public IActionResult GetImage(string imageName)
    //{
    //    var path = Path.Combine(_environment.WebRootPath, imageName);
        
    //    if (!System.IO.File.Exists(path))
    //    {
    //        path = Path.Combine(_environment.WebRootPath, "2el.Az");
    //    }

    //    var imageFileStream = System.IO.File.OpenRead(path);
    //    return File(imageFileStream, "image/jpeg");
    //}
}
