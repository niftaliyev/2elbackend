using System.Text.Json.Serialization;

namespace TwoHandApp.Models;

public class AdImage
{
    public Guid? Id { get; set; }
    public string Url { get; set; } = default!;
    public int AdId { get; set; }
    [JsonIgnore] 

    public Ad Ad { get; set; } = default!;
}