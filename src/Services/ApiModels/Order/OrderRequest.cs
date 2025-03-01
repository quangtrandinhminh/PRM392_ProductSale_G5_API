namespace Services.ApiModels.Order;

public class OrderRequest
{
    public int? CartId { get; set; }

    public int? UserId { get; set; }

    public string PaymentMethod { get; set; }

    public string BillingAddress { get; set; }

    public string OrderStatus { get; set; }

    public DateTime OrderDate { get; set; }
}