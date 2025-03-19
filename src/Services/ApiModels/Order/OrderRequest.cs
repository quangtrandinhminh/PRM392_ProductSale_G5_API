namespace Services.ApiModels.Order;

public class OrderRequest
{
    public int? CartId { get; set; }

    public int? UserId { get; set; }

    public string BillingAddress { get; set; }

}