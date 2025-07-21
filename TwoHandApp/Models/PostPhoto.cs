using System.ComponentModel.DataAnnotations;

namespace TwoHandApp.Models;

public class PostPhoto
{
    public int Id { get; set; }

    [Required]
    public string Url { get; set; }

    public int PostId { get; set; }

    public Post Post { get; set; }
}
