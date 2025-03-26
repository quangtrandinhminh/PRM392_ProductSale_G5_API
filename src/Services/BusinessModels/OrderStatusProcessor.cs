using Microsoft.AspNetCore.Http;
using Repositories.Models;
using Services.Constants;
using Services.Enum;
using Services.Exceptions;
using Services.Utils;

namespace Services.BusinessModels;

public class OrderStatusProcessor(Order order)
{
    public string ChangeNextStatus()
    {
        var status = OrderStatusUltils.TryParseStatus(order.OrderStatus);
        switch (status)
        {
            // system payment success
            case OrderStatusEnum.WaitForPayment:
                Pending();
                break;

            // shop accepted
            case OrderStatusEnum.Pending:
                Approve();
                break;

            // shop shipped
            case OrderStatusEnum.Approved:
                Shipping();
                break;

            // customer received
            case OrderStatusEnum.Shipping:
                Received();
                break;

            // customer returned
            case OrderStatusEnum.Received:
                Received();
                break;
            default:
                throw new AppException(ResponseCodeConstants.BAD_REQUEST, "Invalid status", StatusCodes.Status400BadRequest);
        }

        AddOrderTracking();
        return order.OrderStatus.ToString();
    }

    public void Reject()
    {
        checkStatus(OrderStatusEnum.Pending, OrderStatusEnum.Rejected);

        order.OrderStatus = OrderStatusEnum.Rejected.ToString();
        AddOrderTracking();
    }

    public void Cancel()
    {
        checkStatus(OrderStatusEnum.WaitForPayment, OrderStatusEnum.Cancelled);

        order.OrderStatus = OrderStatusEnum.Cancelled.ToString();
        AddOrderTracking();
    }

    // private void ReturnedWithDamage()
    // {
    //     checkStatus(OrderStatusEnum.Returned, OrderStatusEnum.ReturnedWithDamage);
    //
    //     orderShop.OrderStatus = OrderStatusEnum.ReturnedWithDamage;
    //     AddOrderTracking();
    // }

    private void Pending()
    {
        order.OrderStatus = OrderStatusEnum.Pending.ToString();
    }
    private void Approve()
    {
        order.OrderStatus = OrderStatusEnum.Approved.ToString();
    }

    private void Shipping()
    {
        order.OrderStatus = OrderStatusEnum.Shipping.ToString();
    }

    private void Received()
    {
        order.OrderStatus = OrderStatusEnum.Received.ToString();
    }

    // private void Returning()
    // {
    //     orderShop.OrderStatus = OrderStatusEnum.Returning;
    // }
    //
    // private void Returned()
    // {
    //     orderShop.OrderStatus = OrderStatusEnum.Returned;
    // }
    //
    // private void WaitingForRefund()
    // {
    //     orderShop.OrderStatus = OrderStatusEnum.WaitingForRefund;
    // }

    // private void Completed()
    // {
    //     orderShop.OrderStatus = OrderStatusEnum.Completed;
    // }

    private void AddOrderTracking()
    {
        // orderShop.OrderTrackings.Add(new OrderTracking
        // {
        //     Status = orderShop.OrderStatus.ToString(),
        //     OrderShopId = orderShop.Id,
        // });
    }

    private void checkStatus(OrderStatusEnum statusCurrent, OrderStatusEnum statusTarget)
    {
        if (order.OrderStatus != statusCurrent.ToString())
            throw new AppException(ResponseCodeConstants.BAD_REQUEST,
                               ResponseMessageConstrantsOrder.STATUS_CHANGE_NOTALLOWED + 
                               $" ,Status [{statusCurrent}] Required before to become [{statusTarget}]", 
                               StatusCodes.Status400BadRequest);
    }
}