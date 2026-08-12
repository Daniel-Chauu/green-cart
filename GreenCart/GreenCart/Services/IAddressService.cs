using System.Collections.Generic;
using System.Threading.Tasks;
using GreenCart.Dtos.Requests.Addresses;
using GreenCart.Dtos.Responses.Addresses;

namespace GreenCart.Services
{
    public interface IAddressService
    {
        Task<IEnumerable<AddressResponse>> GetAddressesByUserIdAsync(int userId);
        Task<AddressResponse> GetAddressByIdAsync(int id, int userId);
        Task<AddressResponse> CreateAddressAsync(int userId, CreateAddressRequest request);
        Task<AddressResponse> UpdateAddressAsync(int id, int userId, UpdateAddressRequest request);
        Task DeleteAddressAsync(int id, int userId);
        Task<AddressResponse> SetDefaultAddressAsync(int id, int userId);
    }
}
