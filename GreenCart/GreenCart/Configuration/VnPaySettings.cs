namespace GreenCart.Configuration
{

    public class VnPaySettings
    {
        public string TmnCode { get; set; } = string.Empty;
        public string HashSecret { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
        public string Command { get; set; } = "pay";
        public string CurrCode { get; set; } = "VND";
        public string Version { get; set; } = "2.1.0";
        public string ReturnUrl { get; set; } = "https://localhost:7257/api/payments/vnpay-return";
        public string IpnUrl { get; set; } = "https://localhost:7257/api/payments/vnpay-ipn";
    }
}
