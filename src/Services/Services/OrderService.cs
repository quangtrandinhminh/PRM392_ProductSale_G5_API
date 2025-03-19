using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Repositories.Models;
using Repositories.Repositories;
using Service.ApiModels.VNPay;
using Service.Utils;
using Services.ApiModels.Order;
using Services.BusinessModels;
using Services.Constants;
using Services.Enum;
using Services.Exceptions;
using Services.Mapper;
using Services.Utils;
using ILogger = Serilog.ILogger;

namespace Services.Services;

public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync(OrderRequest request, HttpContext context);
    Task<OrderResponse> GetOrderByIdAsync(int orderId);
    Task<List<OrderResponse>> GetOrdersByUserAsync();
    Task<int> UpdateOrderStatusAsync(int orderId, string newStatus);
    Task<int> DeleteOrderAsync(int orderId);
    Task<int> CustomerChangeOrderStatusAsync(int orderId);
    Task<int> AdminChangeOrderStatusAsync(int orderId);
    Task<int> CustomerCancelOrderAsync(int orderId);
}

public class OrderService(IServiceProvider serviceProvider) : IOrderService
{
    private readonly IOrderRepository _orderRepository = serviceProvider.GetRequiredService<IOrderRepository>();
    private readonly ICartRepository _cartRepository = serviceProvider.GetRequiredService<ICartRepository>();
    private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();
    private readonly MapperlyMapper _mapper = serviceProvider.GetRequiredService<MapperlyMapper>();
    private readonly IVnPayService _vnPayService = serviceProvider.GetRequiredService<IVnPayService>();
    private readonly IHttpContextAccessor _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();

    public async Task<OrderResponse> CreateOrderAsync(OrderRequest request, HttpContext context)
    {
        var loginedUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
        var userId = JwtClaimUltils.GetUserId(loginedUser);

        _logger.Information("Creating new order for UserId: {UserId}", userId);

        var cart = await _cartRepository.GetSingleAsync(x => x.CartId == request.CartId);
        if (cart is null)
        {
            throw new AppException(ResponseCodeConstants.BAD_REQUEST, "Cart does not exist", StatusCodes.Status400BadRequest);
        }

        var order = _mapper.Map(request);
        order.UserId = userId;
        _orderRepository.Create(order);
        await _orderRepository.SaveChangeAsync();

        _logger.Information("Order {OrderId} created successfully", order.OrderId);

        var orderResponse = new OrderResponse();
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
        _logger.Information("Fetching order with Id: {OrderId}", orderId);
        var order = await GetOrderById(orderId);
        return _mapper.Map(order);
    }

    public async Task<List<OrderResponse>> GetOrdersByUserAsync()
    {
        var userId = JwtClaimUltils.GetUserId(JwtClaimUltils.GetLoginedUser(_httpContextAccessor));
        _logger.Information("Fetching orders for UserId: {UserId}", userId);
        var orders = await _orderRepository.GetAllWithCondition(x => x.UserId == userId).ToListAsync();
        return orders.Select(_mapper.Map).ToList();
    }

    public async Task<int> UpdateOrderStatusAsync(int orderId, string newStatus)
    {
        _logger.Information("Updating order {OrderId} status to {Status}", orderId, newStatus);
        var order = await GetOrderById(orderId);

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
        _logger.Information("Deleting order with Id: {OrderId}", orderId);
        var order = await GetOrderById(orderId);

        _orderRepository.Remove(order);
        return await _orderRepository.SaveChangeAsync();
    }

    public async Task<int> CustomerChangeOrderStatusAsync(int orderId)
    {
        _logger.Information("Change order status by id {@orderId}", orderId);
        var userId = JwtClaimUltils.GetUserId(JwtClaimUltils.GetLoginedUser(_httpContextAccessor));
        var order = await GetOrderById(orderId);

        if (order == null)
        {
            throw new AppException(ResponseCodeConstants.NOT_FOUND, ResponseMessageConstrantsOrder.NOT_FOUND,
                               StatusCodes.Status404NotFound);
        }

        // customer can only change status of their own order
        CheckOrderUser(order, userId);

        // order must be in Shipping status
        CheckCustomerOrderStatus(order.OrderStatus);

        var orderStatusProcessor = new OrderStatusProcessor(order);
        orderStatusProcessor.ChangeNextStatus();

        return await _orderRepository.SaveChangeAsync();
    }

    public async Task<int> AdminChangeOrderStatusAsync(int orderId)
    {
        _logger.Information("Change order status by id {@orderId}", orderId);
        var order = await GetOrderById(orderId);
        if (order == null)
        {
            throw new AppException(ResponseCodeConstants.NOT_FOUND, ResponseMessageConstrantsOrder.NOT_FOUND,
                                              StatusCodes.Status404NotFound);
        }

        CheckAdminOrderStatus(order.OrderStatus);

        var orderStatusProcessor = new OrderStatusProcessor(order);
        orderStatusProcessor.ChangeNextStatus();

        return await _orderRepository.SaveChangeAsync();
    }

    public async Task<int> CustomerCancelOrderAsync(int orderId)
    {
        _logger.Information("Cancel order by id {@orderId}", orderId);
        var userId = JwtClaimUltils.GetUserId(JwtClaimUltils.GetLoginedUser(_httpContextAccessor));
        var order = await GetOrderById(orderId);

        if (order == null)
        {
            throw new AppException(ResponseCodeConstants.NOT_FOUND, ResponseMessageConstrantsOrder.NOT_FOUND,
                StatusCodes.Status404NotFound);
        }

        // customer can only cancel their own order
        CheckOrderUser(order, userId);

        // order must be in WaitForPayment status
        if (OrderStatusUltils.TryParseStatus(order.OrderStatus) is not OrderStatusEnum.WaitForPayment)
        {
            throw new AppException(ResponseCodeConstants.BAD_REQUEST, ResponseMessageConstrantsOrder.STATUS_CHANGE_NOTALLOWED,
                StatusCodes.Status400BadRequest);
        }

        return await _orderRepository.SaveChangeAsync();
    }

    private void CheckOrderUser(Order order, int userId)
    {
        if (order.UserId != userId)
        {
            throw new AppException(ResponseCodeConstants.FORBIDDEN, ResponseMessageConstrantsOrder.CUSTOMER_NOTALLOWED,
                               StatusCodes.Status403Forbidden);
        }
    }

    private async Task<Order> GetOrderById(int orderId)
    {
        return await _orderRepository.GetSingleAsync(x => x.OrderId == orderId)
                    ?? throw new AppException(ResponseCodeConstants.NOT_FOUND, ResponseMessageConstrantsOrder.NOT_FOUND, StatusCodes.Status404NotFound);
    }

    private void CheckAdminOrderStatus(string orderStatus)
    {
        List<OrderStatusEnum> validStatuses = new()
        {
            OrderStatusEnum.Pending, OrderStatusEnum.Approved
        };

        if (validStatuses.Contains(OrderStatusUltils.TryParseStatus(orderStatus)))
        {
            throw new AppException(ResponseCodeConstants.BAD_REQUEST, ResponseMessageConstrantsOrder.STATUS_CHANGE_NOTALLOWED,
                                              StatusCodes.Status400BadRequest);
        }
    }

    private void CheckCustomerOrderStatus(string orderStatus)
    {
        List<OrderStatusEnum> validStatuses = new()
        {
            OrderStatusEnum.Shipping
        };

        if (validStatuses.Contains(OrderStatusUltils.TryParseStatus(orderStatus)))
        {
            throw new AppException(ResponseCodeConstants.BAD_REQUEST, ResponseMessageConstrantsOrder.STATUS_CHANGE_NOTALLOWED,
                                                                            StatusCodes.Status400BadRequest);
        }
    }
}