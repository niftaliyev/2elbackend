using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using TwoHandApp.Dtos;
using TwoHandApp.Models;

namespace TwoHandApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly AppDbContext _context;

    public PostsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("with-files")]
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

    [HttpPost]
    public async Task<IActionResult> CreatePost([FromBody] Post post)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Проверка на уникальность номера объявления
        var exists = await _context.Posts.AnyAsync(p => p.PostNumber == post.PostNumber);
        if (exists)
        {
            return Conflict("Post with this PostNumber already exists.");
        }

        // Добавление поста
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
    [HttpGet("mongo/{name}")]
    public async Task<IActionResult> Mongo(string name)
    {
        var client = new MongoClient("mongodb://localhost:27017");
        var database = client.GetDatabase("test");
        var collection = database.GetCollection<BsonDocument>("products");
        var result = collection.Find("{item: 'name'}").ToList();
        var jsonResults = result.Select(doc => BsonExtensionMethods.ToJson(doc));

        return Ok(jsonResults);
    }
}
