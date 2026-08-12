using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GreenCart.Dtos.Requests.Addresses;
using GreenCart.Dtos.Responses.Addresses;
using GreenCart.Entities;
using GreenCart.Repositories;

namespace GreenCart.Services
{
    public class AddressService : IAddressService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddressService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<AddressResponse>> GetAddressesByUserIdAsync(int userId)
        {
            var addresses = await _unitOfWork.ShippingAddresses.FindAsync(a => a.UserId == userId && !a.IsDeleted);
            return addresses.OrderByDescending(a => a.IsDefault).ThenByDescending(a => a.CreatedAt).Select(MapToResponse).ToList();
        }

        public async Task<AddressResponse> GetAddressByIdAsync(int id, int userId)
        {
            var address = await _unitOfWork.ShippingAddresses.GetByIdAsync(id);
            if (address == null || address.IsDeleted || address.UserId != userId)
            {
                throw new KeyNotFoundException($"Shipping address with ID {id} not found.");
            }

            return MapToResponse(address);
        }

        public async Task<AddressResponse> CreateAddressAsync(int userId, CreateAddressRequest request)
        {
            var existingAddresses = (await _unitOfWork.ShippingAddresses.FindAsync(a => a.UserId == userId && !a.IsDeleted)).ToList();

            var isDefault = request.IsDefault || !existingAddresses.Any();

            if (isDefault)
            {
                foreach (var addr in existingAddresses.Where(a => a.IsDefault))
                {
                    addr.IsDefault = false;
                    _unitOfWork.ShippingAddresses.Update(addr);
                }
            }

            var address = new ShippingAddress
            {
                UserId = userId,
                FullName = request.FullName.Trim(),
                PhoneNumber = request.PhoneNumber.Trim(),
                AddressLine1 = request.AddressLine1.Trim(),
                AddressLine2 = request.AddressLine2?.Trim(),
                City = request.City.Trim(),
                State = request.State.Trim(),
                PostalCode = request.PostalCode.Trim(),
                Country = request.Country.Trim(),
                IsDefault = isDefault
            };

            await _unitOfWork.ShippingAddresses.AddAsync(address);
            await _unitOfWork.SaveChangesAsync();

            return MapToResponse(address);
        }

        public async Task<AddressResponse> UpdateAddressAsync(int id, int userId, UpdateAddressRequest request)
        {
            var address = await _unitOfWork.ShippingAddresses.GetByIdAsync(id);
            if (address == null || address.IsDeleted || address.UserId != userId)
            {
                throw new KeyNotFoundException($"Shipping address with ID {id} not found.");
            }

            if (request.IsDefault && !address.IsDefault)
            {
                var existingAddresses = await _unitOfWork.ShippingAddresses.FindAsync(a => a.UserId == userId && !a.IsDeleted);
                foreach (var addr in existingAddresses.Where(a => a.IsDefault))
                {
                    addr.IsDefault = false;
                    _unitOfWork.ShippingAddresses.Update(addr);
                }
            }

            address.FullName = request.FullName.Trim();
            address.PhoneNumber = request.PhoneNumber.Trim();
            address.AddressLine1 = request.AddressLine1.Trim();
            address.AddressLine2 = request.AddressLine2?.Trim();
            address.City = request.City.Trim();
            address.State = request.State.Trim();
            address.PostalCode = request.PostalCode.Trim();
            address.Country = request.Country.Trim();
            address.IsDefault = request.IsDefault;

            _unitOfWork.ShippingAddresses.Update(address);
            await _unitOfWork.SaveChangesAsync();

            return MapToResponse(address);
        }

        public async Task DeleteAddressAsync(int id, int userId)
        {
            var address = await _unitOfWork.ShippingAddresses.GetByIdAsync(id);
            if (address == null || address.IsDeleted || address.UserId != userId)
            {
                throw new KeyNotFoundException($"Shipping address with ID {id} not found.");
            }

            address.IsDeleted = true;
            _unitOfWork.ShippingAddresses.Update(address);

            if (address.IsDefault)
            {
                var remaining = (await _unitOfWork.ShippingAddresses.FindAsync(a => a.UserId == userId && !a.IsDeleted && a.Id != id)).FirstOrDefault();
                if (remaining != null)
                {
                    remaining.IsDefault = true;
                    _unitOfWork.ShippingAddresses.Update(remaining);
                }
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<AddressResponse> SetDefaultAddressAsync(int id, int userId)
        {
            var address = await _unitOfWork.ShippingAddresses.GetByIdAsync(id);
            if (address == null || address.IsDeleted || address.UserId != userId)
            {
                throw new KeyNotFoundException($"Shipping address with ID {id} not found.");
            }

            var existingAddresses = await _unitOfWork.ShippingAddresses.FindAsync(a => a.UserId == userId && !a.IsDeleted);
            foreach (var addr in existingAddresses)
            {
                addr.IsDefault = (addr.Id == id);
                _unitOfWork.ShippingAddresses.Update(addr);
            }

            await _unitOfWork.SaveChangesAsync();

            return MapToResponse(address);
        }

        private static AddressResponse MapToResponse(ShippingAddress address)
        {
            return new AddressResponse
            {
                Id = address.Id,
                UserId = address.UserId,
                FullName = address.FullName,
                PhoneNumber = address.PhoneNumber,
                AddressLine1 = address.AddressLine1,
                AddressLine2 = address.AddressLine2,
                City = address.City,
                State = address.State,
                PostalCode = address.PostalCode,
                Country = address.Country,
                IsDefault = address.IsDefault,
                CreatedAt = address.CreatedAt
            };
        }
    }
}
