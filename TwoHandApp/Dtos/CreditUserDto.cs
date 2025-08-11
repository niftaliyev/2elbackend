namespace TwoHandApp.Dtos;

public class CreditUserDto
{
    public string UserId { get; set; } = default!;
    public decimal Amount { get; set; } // > 0
}

public class PurchaseServiceDto
{
    public string Service { get; set; } = default!; // "premium" или "vip"
}