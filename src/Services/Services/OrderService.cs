using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Repositories.Models;
using Repositories.Repositories;
using Service.ApiModels.VNPay;
using Services.ApiModels.Order;
using Services.Constants;
using Services.Enum;
using Services.Exceptions;
using Services.Mapper;

namespace Services.Services;

public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync(OrderRequest request, HttpContext context);
    Task<OrderResponse> GetOrderByIdAsync(int orderId);
    Task<List<OrderResponse>> GetOrdersByUserAsync(int userId);
    Task<int> UpdateOrderStatusAsync(int orderId, string newStatus);
    Task<int> DeleteOrderAsync(int orderId);

    Task<List<OrderResponse>> GetOrdersByStatusAsync(string status);
}

public class OrderService(IServiceProvider serviceProvider, IConfiguration configuration) : IOrderService
{
    private readonly IOrderRepository _orderRepository = serviceProvider.GetRequiredService<IOrderRepository>();
    private readonly IUserRepository _userRepository = serviceProvider.GetRequiredService<IUserRepository>();
    private readonly ICartRepository _cartRepository = serviceProvider.GetRequiredService<ICartRepository>();
    private readonly ILogger<OrderService> _logger = serviceProvider.GetRequiredService<ILogger<OrderService>>();
    private readonly MapperlyMapper _mapper = serviceProvider.GetRequiredService<MapperlyMapper>();
    private readonly IVnPayService _vnPayService = serviceProvider.GetRequiredService<IVnPayService>();
    private readonly IConfiguration _configuration = configuration;

    public async Task<OrderResponse> CreateOrderAsync(OrderRequest request, HttpContext context)
    {
        _logger.LogInformation("Creating new order for UserId: {UserId}", request.UserId);

        var user = await _userRepository.GetSingleAsync(x => x.UserId == request.UserId)
                   ?? throw new AppException(ResponseCodeConstants.BAD_REQUEST, "User does not exist", StatusCodes.Status400BadRequest);

        Cart cart = null;
        if (request.CartId.HasValue)
        {
            cart = await _cartRepository.GetSingleAsync(x => x.CartId == request.CartId);
            if (cart is null)
            {
                throw new AppException(ResponseCodeConstants.BAD_REQUEST, "Cart does not exist", StatusCodes.Status400BadRequest);
            }
        }

        // Create an Order object from Repositories.Models
        var order = new Order
        {
            UserId = request.UserId.Value,
            CartId = request.CartId,
            PaymentMethod = "VnPay",
            BillingAddress = request.BillingAddress,
            OrderStatus = OrderShopStatusEnum.WaitForPayment.ToString(),
            OrderDate = DateTime.Now,
        };

        _orderRepository.Create(order);
        await _orderRepository.SaveChangeAsync();

        _logger.LogInformation("Order {OrderId} created successfully", order.OrderId);

        var orderResponse = _mapper.Map(order);

        if (orderResponse.PaymentMethod == PaymentMethodEnum.VnPay.ToString())
        {
            // Use IVnPayService to create the payment URL
            var vnPayRequest = new VnPaymentRequest
            {
                OrderId = order.OrderId,
            };

            orderResponse.PaymentUrl = await _vnPayService.CreatePaymentUrl(context, vnPayRequest);
        }

        return orderResponse;
    }
    public async Task<OrderResponse> GetOrderByIdAsync(int orderId)
    {
        _logger.LogInformation("Fetching order with Id: {OrderId}", orderId);
        var order = await _orderRepository.GetSingleAsync(x => x.OrderId == orderId)
                    ?? throw new AppException(ResponseCodeConstants.NOT_FOUND, "Order not found", StatusCodes.Status404NotFound);

        return _mapper.Map(order);
    }

    public async Task<List<OrderResponse>> GetOrdersByUserAsync(int userId)
    {
        _logger.LogInformation("Fetching orders for UserId: {UserId}", userId);
        var orders = await _orderRepository.GetAllWithCondition(x => x.UserId == userId).ToListAsync();
        return orders.Select(_mapper.Map).ToList();
    }

    public async Task<int> UpdateOrderStatusAsync(int orderId, string newStatus)
    {
        _logger.LogInformation("Updating order {OrderId} status to {Status}", orderId, newStatus);
        var order = await _orderRepository.GetSingleAsync(x => x.OrderId == orderId)
                    ?? throw new AppException(ResponseCodeConstants.NOT_FOUND, "Order not found", StatusCodes.Status404NotFound);

        var validStatuses = new List<string> { "Pending", "Paid", "Shipped", "Completed", "Cancelled" };
        if (!validStatuses.Contains(newStatus))
        {
            throw new AppException(ResponseCodeConstants.BAD_REQUEST, "Invalid order status", StatusCodes.Status400BadRequest);
        }

        order.OrderStatus = newStatus;
        _orderRepository.Update(order);
        return await _orderRepository.SaveChangeAsync();
    }

    public async Task<int> DeleteOrderAsync(int orderId)
    {
        _logger.LogInformation("Deleting order with Id: {OrderId}", orderId);
        var order = await _orderRepository.GetSingleAsync(x => x.OrderId == orderId)
                    ?? throw new AppException(ResponseCodeConstants.NOT_FOUND, "Order not found", StatusCodes.Status404NotFound);

        _orderRepository.Remove(order);
        return await _orderRepository.SaveChangeAsync();
    }

    public async Task<List<OrderResponse>> GetOrdersByStatusAsync(string status)
    {
        // Kiểm tra trạng thái hợp lệ
        var validStatuses = new List<string> { "Pending", "Paid", "Shipped", "Completed", "Cancelled" };
        if (!validStatuses.Contains(status))
        {
            throw new AppException(ResponseCodeConstants.BAD_REQUEST, "Invalid order status", StatusCodes.Status400BadRequest);
        }

        // Lấy danh sách đơn hàng từ repository, bao gồm thông tin User và Cart
        var orders = await _orderRepository.GetAllWithCondition(x => x.OrderStatus == status)
                                           .Include(o => o.User)
                                           .Include(o => o.Cart)
                                           .ToListAsync();

        // Ánh xạ sang OrderResponse
        var orderResponses = orders.Select(order => new OrderResponse
        {
            OrderId = order.OrderId,
            CartId = order.CartId,
            UserId = order.UserId,
            PaymentMethod = order.PaymentMethod,
            BillingAddress = order.BillingAddress,
            OrderStatus = order.OrderStatus,
            OrderDate = order.OrderDate,

            // Thông tin User
            CustomerName = order.User?.Username,
            CustomerEmail = order.User?.Email,
            CustomerPhone = order.User?.PhoneNumber,

            // Thông tin Cart
            CartTotalAmount = order.Cart?.TotalPrice
        }).ToList(); // Fix lỗi ép kiểu

        return orderResponses;
    }
}