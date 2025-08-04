namespace TwoHandApp.Models;

public class UserDetailsDto
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }

    public List<string> Roles { get; set; }
    public List<string> Permissions { get; set; }
}

