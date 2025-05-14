namespace Services.ApiModels.Order;

public class OrderResponse
{
    public int OrderId { get; set; } // ID của đơn hàng
    public int? CartId { get; set; } // ID của giỏ hàng liên quan
    public int? UserId { get; set; } // ID của người dùng đặt hàng
    public string PaymentMethod { get; set; } // Phương thức thanh toán
    public string BillingAddress { get; set; } // Địa chỉ thanh toán
    public string OrderStatus { get; set; } // Trạng thái đơn hàng
    public DateTime OrderDate { get; set; } // Ngày đặt hàng

    public string PaymentUrl { get; set; } // URL thanh toán

    // Thông tin User
    public string? CustomerName { get; set; } // Tên khách hàng
    public string? CustomerEmail { get; set; } // Email khách hàng
    public string? CustomerPhone { get; set; } // Số điện thoại khách hàng

    // Thông tin Cart
    public decimal? CartTotalAmount { get; set; } // Tổng số tiền của giỏ hàng
}