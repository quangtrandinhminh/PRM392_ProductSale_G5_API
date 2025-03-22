using Services.ApiModels.Product;

namespace Services.ApiModels.CartItem
{
    public class CartItemResponse
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public ProductResponse Product { get; set; }
    }
}
