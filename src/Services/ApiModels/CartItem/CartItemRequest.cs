using System.ComponentModel.DataAnnotations;

namespace Services.ApiModels.CartItem;

public class CartItemRequest
{
    public int ProductId { get; set; }

    [Range(0.0001, int.MaxValue)]
    public int Quantity { get; set; }
}