namespace GreenCart.Dtos.Responses.Payments
{
    public class VnPayReturnResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "VNPAY";
    }
}
