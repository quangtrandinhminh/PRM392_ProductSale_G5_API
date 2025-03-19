namespace Service.ApiModels.VNPay
{
    public class VnPaymentRequest
    {
        // Mã đơn hàng của merchant
        public int OrderId { get; set; }

        // Địa chỉ URL trả về khi thanh toán thành công
        public string ReturnUrl { get; set; }

        // ID người dùng đăng nhập Web/App
        public string UserId { get; set; }

        // Mã ngân hàng hoặc loại thẻ thanh toán (tùy chọn)
        public string BankCode { get; set; }

        // Loại thẻ thanh toán: Nội địa (01), Quốc tế (02)
        public string CardType { get; set; }

        // Mã giao dịch duy nhất
        public string TxnRef { get; set; }

        // Số tiền thanh toán (thanh toán bằng VND, nhân 100 để chuyển đổi)
        public long Amount { get; set; }

        // Mô tả giao dịch
        public string TxnDesc { get; set; }

        // Ngôn ngữ giao diện thanh toán ("vn" cho Tiếng Việt, "en" cho Tiếng Anh)
        public string Locale { get; set; }

        // Địa chỉ IP của khách hàng thực hiện giao dịch
        public string IpAddr { get; set; }

        // Thời gian giao dịch theo định dạng yyyyMMddHHmmss
        public string CreateDate { get; set; }

        // Lưu token hay không (mặc định là 1 lưu token sau khi thanh toán thành công)
        public bool StoreToken { get; set; } = true;

        // URL trả về khi thanh toán bị hủy (tùy chọn)
        public string CancelUrl { get; set; }

        // Mã checksum để bảo vệ tính toàn vẹn của dữ liệu
        public string SecureHash { get; set; }
    }
}
