using Microsoft.Extensions.DependencyInjection;
using Service.ApiModels.VNPay;
using Services.Config;
using Services.Constants;
using Services.Enum;
using Services.Exceptions;
using Services.Helper;
using Repositories.Repositories;
using Microsoft.AspNetCore.Http;

namespace Services.Services
{
    public interface IVnPayService
    {
        Task<string> CreatePaymentUrl(HttpContext context, VnPaymentRequest request);
        Task<VnPaymentResponse> PaymentExecute(HttpContext context);
    }

    public class VnPayService : IVnPayService
    {
        private readonly VnPaySetting _vnpaySetting;
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentRepository _paymentRepository;

        public VnPayService(IServiceProvider serviceProvider)
        {
            _vnpaySetting = VnPaySetting.Instance;
            _orderRepository = serviceProvider.GetRequiredService<IOrderRepository>();
            _paymentRepository = serviceProvider.GetRequiredService<IPaymentRepository>();
        }

        public async Task<string> CreatePaymentUrl(HttpContext context, VnPaymentRequest request)
        {
            string returnUrl = $"https://localhost:7180/api/paymentvnpay/payment-execute";
            string hostName = System.Net.Dns.GetHostName();
            string clientIPAddress = System.Net.Dns.GetHostAddresses(hostName).GetValue(0).ToString();

            var order = await GetOrderById(request.OrderId);

            if (order == null)
            {
                throw new InvalidOperationException("Order not found.");
            }

            var tick = order.OrderId;
            var vnpay = new VnPayLibrary();

            // Cấu hình dữ liệu
            vnpay.AddRequestData("vnp_Version", "2.1.0"); // Version
            vnpay.AddRequestData("vnp_Command", "pay"); // Command for create token
            vnpay.AddRequestData("vnp_TmnCode", _vnpaySetting.TmnCode); // Merchant code
            vnpay.AddRequestData("vnp_BankCode", "");
            vnpay.AddRequestData("vnp_Locale", "vn");
            var amount = order.Cart.TotalPrice * 100; // Convert to VND in cents

            // Ensure amount is a whole number (integer), not a decimal or float
            int amountInCents = (int)amount;  // Convert to integer

            if (amountInCents <= 0)
            {
                throw new InvalidOperationException("Invalid amount.");
            }

            vnpay.AddRequestData("vnp_Amount", amountInCents.ToString());
            DateTime createDate = DateTime.Now;
            vnpay.AddRequestData("vnp_CreateDate", createDate.ToString("yyyyMMddHHmmss")); 


            // Tính toán giá trị thanh toán
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_TxnRef", tick.ToString());
            vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toán đơn hàng ID: {request.OrderId}, Tổng giá trị: {order.Cart.TotalPrice} VND");
            vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
            vnpay.AddRequestData("vnp_IpAddr", clientIPAddress);
            vnpay.AddRequestData("vnp_OrderType", "other");

            try
            {
                // Create payment entity in the repository
                var payment = new Repositories.Models.Payment
                {
                    OrderId = order.OrderId,
                    Amount = amountInCents,
                    PaymentStatus = PaymentStatusEnum.Pending.ToString(),
                    PaymentDate = createDate
                };

                 _paymentRepository.Create(payment);
                await _paymentRepository.SaveChangeAsync();


                // Generate the payment URL
                var paymentUrl = vnpay.CreateRequestUrl(_vnpaySetting.BaseUrl, _vnpaySetting.HashSecret);
                return paymentUrl;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error creating payment URL", ex);
            }
        }

        public async Task<VnPaymentResponse> PaymentExecute(HttpContext context)
        {
            var vnpay = new VnPayLibrary();

            // Retrieve vnp_TxnRef from the query string directly
            var vnpOrderIdStr = context.Request.Query["vnp_TxnRef"].ToString();
            int vnpOrderId = 0;

            // Validate vnpOrderId
            if (string.IsNullOrEmpty(vnpOrderIdStr) || !int.TryParse(vnpOrderIdStr, out vnpOrderId))
            {
                return new VnPaymentResponse()
                {
                    Success = false,
                    Message = "Invalid or missing order ID"
                };
            }

            var order = await GetOrderById(vnpOrderId);
            var payment = await GetPaymentByOrderId(vnpOrderId);
            if (order == null)
            {
                return new VnPaymentResponse()
                {
                    Success = false,
                    Message = "Order not found"
                };
            }

            var request = context.Request;
            var collections = request.Query;

            // Add all parameters starting with "vnp_" to the vnpay instance
            foreach (var (key, value) in collections)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value.ToString());
                }
            }

            // Extract necessary data from the response
            var vnpTransactionId = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));
            var vnpSecureHash = collections.FirstOrDefault(p => p.Key == "vnp_SecureHash").Value;
            var vnpResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            var vnpOrderInfo = vnpay.GetResponseData("vnp_OrderInfo");

            // Validate the signature
            bool checkSignature = vnpay.ValidateSignature(vnpSecureHash, _vnpaySetting.HashSecret);
            if (!checkSignature)
            {
                return new VnPaymentResponse()
                {
                    Success = false,
                    Message = "Invalid signature"
                };
            }

            // Handle the response based on the VNPay response code
            if (vnpResponseCode == "00") // Success
            {
                // Update order status to "Pending" or any relevant status
                order.PaymentMethod = PaymentMethodEnum.VnPay.ToString();
                order.OrderStatus = OrderStatusEnum.Pending.ToString();
                _orderRepository.Update(order);
                payment.PaymentStatus = PaymentStatusEnum.Paid.ToString();
                _paymentRepository.Update(payment);
                await _paymentRepository.SaveChangeAsync();

                await _orderRepository.SaveChangeAsync();
            }
            else
            {
                // Handle other response codes
                return new VnPaymentResponse()
                {
                    Success = false,
                    Message = $"Payment failed with response code: {vnpResponseCode}"
                };
            }
            return new VnPaymentResponse()
            {
                Success = true,
                PaymentMethod = "VnPay",
                OrderDescription = vnpOrderInfo,
                OrderId = vnpOrderId.ToString(),
                TransactionId = vnpTransactionId.ToString(),
                Token = vnpSecureHash,
                PaymentId = request.QueryString.ToString(),
                VnPayResponseCode = vnpResponseCode
            };
        }

        private async Task<Repositories.Models.Order> GetOrderById(int orderId)
        {
            var order = await _orderRepository.GetSingleAsync(
                x => x.OrderId == orderId,
                o => o.Cart
            );

            if (order == null)
            {
                throw new AppException(ResponseCodeConstants.NOT_FOUND, ResponseMessageConstrantsOrder.NOT_FOUND, StatusCodes.Status404NotFound);
            }

            // Return the order when it's found
            return order;
        }


        private async Task<Repositories.Models.Payment> GetPaymentByOrderId(int orderId)
        {
            var payment = await _paymentRepository.GetSingleAsync(
                x => x.OrderId == orderId
            );

            if (payment == null)
            {
                throw new AppException(ResponseCodeConstants.NOT_FOUND, ResponseMessageConstrantsOrder.NOT_FOUND, StatusCodes.Status404NotFound);
            }

            // Ensure that we return the payment when it's found
            return payment;
        }

    }
}
