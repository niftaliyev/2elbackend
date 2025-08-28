using TwoHandApp.Enums;

namespace TwoHandApp.Models;

public class PackagePrice
{
    public Guid Id { get; set; }
    public int? IntervalDay { get; set; }
    public int? IntervalHours { get; set; }
    public int? BoostCount { get; set; }
    public decimal? Price { get; set; }
    public PackageType PackageType { get; set; }
    public string Description { get; set; }
}