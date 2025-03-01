namespace Repositories.Extensions;

public class RevenueChartResponse
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalRevenue { get; set; } = 0;
    public int TotalOrder { get; set; } = 0;
}