using System.Threading.Tasks;
using GreenCart.Dtos.Requests.Auth;
using GreenCart.Dtos.Responses.Auth;
using GreenCart.Entities.Enums;

namespace GreenCart.Services
{
    public interface IUserService
    {
        Task<AuthResponse> RegisterStaffAsync(RegisterStaffRequest request);
        Task<UserProfileResponse> UpdateUserRoleAsync(int adminUserId, int targetUserId, UserRole newRole);
    }
}
