using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TwoHandApp.Dtos;

public class CreatePostWithFilesDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Required]
    [MaxLength(50)]
    public string PosterType { get; set; }

    [Phone]
    public string? ContactNumber { get; set; }

    public bool AllowMessages { get; set; }

    [Required]
    public string City { get; set; }

    public bool IsNew { get; set; }

    [Required]
    public string ProductType { get; set; }

    public bool HasDelivery { get; set; }

    [Required]
    public string Description { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    [Phone]
    public string? Mobile { get; set; }
}

