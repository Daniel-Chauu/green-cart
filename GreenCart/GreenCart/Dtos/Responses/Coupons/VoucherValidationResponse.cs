namespace GreenCart.Dtos.Responses.Coupons
{
    public class VoucherValidationResponse
    {
        public bool IsValid { get; set; }
        public string? Message { get; set; }
        public decimal? DiscountAmount { get; set; }
        public string? DiscountType { get; set; }
        public string? VoucherCode { get; set; }
    }
}
