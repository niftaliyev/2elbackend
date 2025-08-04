namespace TwoHandApp.Models;

public class AdPhoto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = default!;
    public bool IsMain { get; set; }

    public Guid AdId { get; set; }
    public Ad Ad { get; set; } = default!;
}