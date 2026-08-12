using System.Threading.Tasks;
using GreenCart.Dtos.Requests.Auth;
using GreenCart.Dtos.Responses.Auth;

namespace GreenCart.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RefreshTokenAsync(string refreshToken);
        Task LogoutAsync(int userId, string refreshToken);
        Task ChangePasswordAsync(int userId, ChangePasswordRequest request);
        Task<UserProfileResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request);
        Task<UserProfileResponse> GetCurrentUserAsync(int userId);
        Task ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<VerifyResetCodeResponse> VerifyResetCodeAsync(VerifyResetCodeRequest request);
        Task ResetPasswordAsync(ResetPasswordRequest request);
    }
}
