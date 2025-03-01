using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Repositories.Base;
using Repositories.Models;
using Service.ApiModels.VNPay;
using Services.Config;
using Services.Constants;
using Services.Enum;
using Services.Exceptions;
using Services.Helper;

namespace Services.Services;

public class VnPayService(IServiceProvider serviceProvider)
{
    private readonly VnPaySetting _vnpaySetting = VnPaySetting.Instance;
    private readonly GenericRepository<Order> _orderRepository = serviceProvider.GetRequiredService<GenericRepository<Order>>();

    public async Task<string> CreatePaymentUrl(HttpContext context, VnPaymentRequest request)
    {
        var order = await GetOrderById(request.OrderId);

        var tick = DateTime.Now.Ticks.ToString();
        var vnpay = new VnPayLibrary();
        vnpay.AddRequestData("vnp_Version", _vnpaySetting.Version);
        vnpay.AddRequestData("vnp_Command", VnPayCommandEnum.pay.ToString());
        vnpay.AddRequestData("vnp_TmnCode", _vnpaySetting.TmnCode);

        //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn,
        //ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân),
        //sau đó gửi sang VNPAY là: 10000000
        vnpay.AddRequestData("vnp_Amount",
            (order.Cart.TotalPrice * 100)
            .ToString());

        vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
        vnpay.AddRequestData("vnp_CurrCode", _vnpaySetting.CurrCode);
        vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(context));
        vnpay.AddRequestData("vnp_Locale", _vnpaySetting.Locale);

        vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan hoa don " + request.OrderId);
        vnpay.AddRequestData("vnp_OrderType", "other"); //default value: other
        vnpay.AddRequestData("vnp_ReturnUrl", request.ReturnUrl);

        // Mã tham chiếu của giao dịch tại hệ thống của merchant.
        // Mã này là duy nhất dùng để phân biệt các đơn hàng gửi sang VNPAY.
        // Không được trùng lặp trong ngày
        vnpay.AddRequestData("vnp_TxnRef",
            tick);

        var paymentUrl = vnpay.CreateRequestUrl(_vnpaySetting.BaseUrl, _vnpaySetting.HashSecret);

        return paymentUrl;
    }

    // returnUrl call this method
    public async Task<VnPaymentResponse> PaymentExecute(HttpContext context, int orderId)
    {
        var order = await GetOrderById(orderId);

        var request = context.Request;
        var collections = request.Query;
        var vnpay = new VnPayLibrary();

        foreach (var (key, value) in collections)
        {
            if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
            {
                vnpay.AddResponseData(key, value.ToString());
            }
        }

        var vnp_orderId = Convert.ToInt64(vnpay.GetResponseData("vnp_TxnRef"));
        var vnp_TransactionId = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));
        var vnp_SecureHash = collections.FirstOrDefault(p => p.Key == "vnp_SecureHash").Value;
        var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
        var vnp_OrderInfo = vnpay.GetResponseData("vnp_OrderInfo");

        bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, _vnpaySetting.HashSecret);
        if (!checkSignature)
        {
            return new VnPaymentResponse()
            {
                Success = false
            };
        }

        if (vnp_ResponseCode == "00")
        {
            // update order status
            order.PaymentMethod = PaymentMethodEnum.VnPay.ToString();
            order.OrderStatus = OrderShopStatusEnum.Pending.ToString();
            _orderRepository.Update(order);
            await _orderRepository.SaveChangeAsync();
        }


        return new VnPaymentResponse()
        {
            Success = true,
            PaymentMethod = "VnPay",
            OrderDescription = vnp_OrderInfo,
            OrderId = vnp_orderId.ToString(),
            TransactionId = vnp_TransactionId.ToString(),
            Token = vnp_SecureHash,
            PaymentId = request.QueryString.ToString(),

            // success true => 00 , false => != 00
            VnPayResponseCode = vnp_ResponseCode
        };
    }

    private async Task<Order> GetOrderById(int orderId)
    {
        var order = await _orderRepository.GetSingleAsync(x => x.OrderId == orderId);
        if (order == null)
        {
            throw new AppException(ResponseCodeConstants.NOT_FOUND, ResponseMessageConstrantsOrder.NOT_FOUND, StatusCodes.Status404NotFound);
        }

        return order;
    }
}