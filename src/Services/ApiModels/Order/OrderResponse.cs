namespace Services.ApiModels.Order;

public class OrderResponse
{
    public string? PaymentUrl { get; set; }
    public string? PaymentMethod { get; set; }
}