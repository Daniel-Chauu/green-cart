namespace GreenCart.Dtos.Responses.Payments
{
    public class VnPayPaymentResponse
    {
        public string PaymentUrl { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
