namespace Services.ApiModels.Order;

public class OrderResponse
{
    public decimal ProvisionalTotal { get; set; }
    public decimal ShipFee { get; set; }
    public decimal ShipSupportFee { get; set; }
    public decimal Tax { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TotalDiscount { get; set; }
    public string? PaymentId { get; set; }
    public string? Note { get; set; }
    // public IList<VoucherResponse> PlatformVouchers { get; set; } = new List<VoucherResponse>();
    // public IList<OrderShopListResponse> OrderShops { get; set; } = new List<OrderShopListResponse>();
    public string? PaymentUrl { get; set; }
    public string? PaymentMethod { get; set; }
}