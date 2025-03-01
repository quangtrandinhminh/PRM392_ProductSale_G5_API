namespace Services.ApiModels.Cart;

public class CartResponse
{
    public int CartId { get; set; }

    public int? UserId { get; set; }

    public decimal TotalPrice { get; set; }

    public string Status { get; set; }
}