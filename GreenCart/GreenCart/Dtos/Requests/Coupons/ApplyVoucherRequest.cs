using System.ComponentModel.DataAnnotations;

namespace GreenCart.Dtos.Requests.Coupons
{
    public class ApplyVoucherRequest
    {
        [Required]
        public string VoucherCode { get; set; } = string.Empty;
    }
}
