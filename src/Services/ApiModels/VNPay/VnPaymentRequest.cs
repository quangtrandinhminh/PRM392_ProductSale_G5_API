namespace Service.ApiModels.VNPay;

public class VnPaymentRequest
{
    public int OrderId { get; set; }
    public string ReturnUrl { get; set; }
}