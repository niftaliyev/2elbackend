using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using TwoHandApp.Models;

namespace TwoHandApp.Controllers;
[ApiController]
[Route("api/[controller]")]
public class AdvertisementsController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly AppDbContext _context;

    public AdvertisementsController(IWebHostEnvironment environment, AppDbContext context)
    {
        _environment = environment;
        this._context = context;
    }
    [HttpGet("id/{id}")]
    public ActionResult<IEnumerable<Advertisement>> GetAdvertisement(int id)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var advertisements = _context.Posts.FirstOrDefault(x => x.Id == id);

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
        var advertisements = _context.Advertisements.ToList().Take(4);

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
        var advertisements2 = _context.Posts.ToList();

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

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreatePostWithFiles(
    [FromForm] Post post,
    List<IFormFile> photos)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Проверка на уникальность номера объявления
        var exists = await _context.Posts.AnyAsync(p => p.PostNumber == post.PostNumber);
        if (exists)
        {
            return Conflict("Post with this PostNumber already exists.");
        }

        // Сохраняем файлы как URL (например, в wwwroot/images)
        string? imagePath = null;
        bool turn = true;
        foreach (var photo in photos)
        {
            if (photo.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(photo.FileName);
                if (turn)
                {
                    imagePath = fileName;
                    var savePath1 = Path.Combine("wwwroot", fileName);
                    using var stream1 = new FileStream(savePath1, FileMode.Create);
                    await photo.CopyToAsync(stream1);

                }
                else
                    turn = false;
                var savePath = Path.Combine("wwwroot/images", fileName);
                Directory.CreateDirectory("wwwroot/images");
                using var stream = new FileStream(savePath, FileMode.Create);
                await photo.CopyToAsync(stream);
                //post.Photos.Add(new PostPhoto
                //{
                //    Url = "/images/" + fileName
                //});
            }
        }
        // Добавление поста
        post.imageUrl = $"{imagePath}" ?? "default.jpg";
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<Post>> GetPost(int id)
    {
        var post = await _context.Posts/*
            .Include(p => p.Photos)*/
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
            return NotFound();

        return post;
    }
    //[HttpGet("mongo/{name}")]
    //public async Task<IActionResult> Mongo(string name)
    //{
    //    var client = new MongoClient("mongodb://localhost:27017");
    //    var database = client.GetDatabase("test");
    //    var collection = database.GetCollection<BsonDocument>("products");
    //    var result = collection.Find("{item: 'name'}").ToList();
    //    var jsonResults = result.Select(doc => BsonExtensionMethods.ToJson(doc));

    //    return Ok(jsonResults);
    //}
}
