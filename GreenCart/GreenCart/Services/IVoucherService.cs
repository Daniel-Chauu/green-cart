using System.Threading.Tasks;
using GreenCart.Dtos.Requests.Coupons;
using GreenCart.Dtos.Responses.Coupons;

namespace GreenCart.Services
{
    public interface IVoucherService
    {
        Task<VoucherValidationResponse> ValidateVoucherAsync(int userId, ApplyVoucherRequest request);
    }
}
