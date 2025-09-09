using System.ComponentModel.DataAnnotations;

namespace TwoHandApp.Models;

public class IncreaseBalance
{
    [Key]
    public int Id { get; set; }
    public string UserId { get; set; } = default!;
    public ApplicationUser User { get; set; } = default!;
    public int Amount { get; set; }
    public string? Image { get; set; }
    public string? Name { get; set; }
}