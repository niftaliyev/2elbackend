using TwoHandApp.Enums;

namespace TwoHandApp.Models;

public class UserAdPackage
{
    public Guid Id { get; set; }

    public int AdId { get; set; }

    public Ad Ad { get; set; }

    public Guid PackagePriceId { get; set; }

    public PackagePrice PackagePrice { get; set; }

    public decimal? PaidPrice { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; } 
    public int? BoostsRemaining { get; set; }    
    public DateTime? LastBoostedAt { get; set; }  
    public PackageType Type { get; set; }

}