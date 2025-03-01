namespace Repositories.Extensions;

public class AdminRevenueStatisticResponse
{
    public decimal TotalRevenue { get; set; } = 0;

    // count ordershop result set in date range
    public int TotalOrder { get; set; } = 0;

    // count total shop result set 
    public int TotalShop { get; set; } = 0;

    // count total product result set
    public int TotalProduct { get; set; } = 0;
    public int TotalUser { get; set; } = 0;
}