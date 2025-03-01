namespace Repositories.Extensions;

public class ShopRevenueStatisticResponse
{
    public decimal TotalRevenue { get; set; }

    // count ordershop result set in date range
    public int TotalOrder { get; set; }

    // count total product result set
    public int TotalProduct { get; set; }
}