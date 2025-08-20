using System.ComponentModel.DataAnnotations;

namespace TwoHandApp.Models;

public class UserRefreshToken
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; }  // FK на AspNetUsers.Id

    [Required]
    public string Token { get; set; }   // сам refresh_token (GUID или JWT)

    [Required]
    public DateTime ExpiresAt { get; set; }

    public bool IsRevoked { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
