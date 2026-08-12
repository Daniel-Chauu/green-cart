using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GreenCart.Dtos.Requests.Auth;
using GreenCart.Dtos.Responses.Auth;
using GreenCart.Entities;
using GreenCart.Entities.Enums;
using GreenCart.Repositories;
using GreenCart.Services.Common;

namespace GreenCart.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;

        public UserService(IUnitOfWork unitOfWork, ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
        }

        public async Task<AuthResponse> RegisterStaffAsync(RegisterStaffRequest request)
        {
            
            if (request.Role != UserRole.Staff && request.Role != UserRole.Admin)
            {
                throw new InvalidOperationException("Role must be either Staff or Admin.");
            }

            
            var emailExists = await _unitOfWork.Users.ExistsAsync(u => u.Email.ToLower() == request.Email.Trim().ToLower());
            if (emailExists)
            {
                throw new InvalidOperationException("An account with this email already exists.");
            }

            
            var user = new User
            {
                FullName = request.FullName.Trim(),
                Email = request.Email.Trim().ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                PhoneNumber = request.PhoneNumber?.Trim(),
                Address = request.Address?.Trim(),
                Role = request.Role
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            
            var token = _tokenService.GenerateAccessToken(user);

            return new AuthResponse
            {
                AccessToken = token,
                ExpiresIn = 3600,
                User = MapToUserProfile(user)
            };
        }

        public async Task<UserProfileResponse> UpdateUserRoleAsync(int adminUserId, int targetUserId, UserRole newRole)
        {
            var targetUser = await _unitOfWork.Users.GetByIdAsync(targetUserId);
            if (targetUser == null)
            {
                throw new KeyNotFoundException($"User with ID {targetUserId} not found.");
            }

            
            if (adminUserId == targetUserId && newRole != UserRole.Admin)
            {
                throw new InvalidOperationException("You cannot demote your own admin account.");
            }

            targetUser.Role = newRole;
            targetUser.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Users.Update(targetUser);
            await _unitOfWork.SaveChangesAsync();

            return MapToUserProfile(targetUser);
        }

        private static UserProfileResponse MapToUserProfile(User user)
        {
            return new UserProfileResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt
            };
        }
    }
}
