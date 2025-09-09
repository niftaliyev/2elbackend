namespace TwoHandApp.Dto;

public class PackagePriceDto
{
    public int? IntervalDay { get; set; }
    public int? IntervalHours { get; set; }
    public int? BoostCount { get; set; }
    public decimal? Price { get; set; }
    public int PackageType { get; set; }
    public string Description { get; set; }
}

public class PackageVipProDto
{
    public int? IntervalDay { get; set; }
    public decimal? Price { get; set; }
    public string Description { get; set; }
}