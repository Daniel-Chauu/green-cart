using System.Threading;
using System.Threading.Tasks;
using GreenCart.Dtos.Responses.Payments;
using GreenCart.Entities.Enums;
using Microsoft.AspNetCore.Http;

namespace GreenCart.Services
{
    public interface IVnPayService
    {
        Task<VnPayPaymentResponse> CreatePaymentUrlAsync(int orderId, int userId, string ipAddress, CancellationToken cancellationToken = default);
        Task<VnPayReturnResponse> HandleReturnAsync(IQueryCollection query, CancellationToken cancellationToken = default);
        Task<VnPayIpnResponse> HandleIpnAsync(IQueryCollection query, CancellationToken cancellationToken = default);
        Task<PaymentStatus> GetTransactionStatusAsync(int orderId, CancellationToken cancellationToken = default);
    }
}
