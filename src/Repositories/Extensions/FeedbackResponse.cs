namespace Repositories.Extensions;

public class FeedbackResponse
{
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? Avatar { get; set; }
    public int? ProductId { get; set; }
    public string? ProductName { get; set; }
    public int OrderDetailId { get; set; }
    public byte Rating { get; set; }
    public string? ReviewContent { get; set; }
    public DateTimeOffset? FeedbackCreatedTime { get; set; }
    public DateTimeOffset? FeedbackLastUpdatedTime { get; set; }
}