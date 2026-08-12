using System.ComponentModel.DataAnnotations;

namespace GreenCart.Dtos.Requests.Coupons
{
    public class ApplyVoucherRequest
    {
        public string VoucherCode { get; set; } = string.Empty;
    }
}
