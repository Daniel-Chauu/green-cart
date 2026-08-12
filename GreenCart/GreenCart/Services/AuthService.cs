using System;
using System.Linq;
using System.Threading.Tasks;
using GreenCart.Configuration;
using GreenCart.Dtos.Requests.Auth;
using GreenCart.Dtos.Responses.Auth;
using GreenCart.Entities;
using GreenCart.Entities.Enums;
using GreenCart.Repositories;
using GreenCart.Services.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GreenCart.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly AppSettings _appSettings;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            IEmailService emailService,
            IOptions<AppSettings> appSettings,
            ILogger<AuthService> logger)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _emailService = emailService;
            _appSettings = appSettings.Value;
            _logger = logger;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            
            var emailExists = await _unitOfWork.Users.ExistsAsync(u => u.Email.ToLower() == request.Email.ToLower());
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
                Role = UserRole.Customer 
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            
            var token = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                ExpiresIn = 900, 
                User = MapToUserProfile(user)
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            
            var users = await _unitOfWork.Users.FindAsync(u => u.Email.ToLower() == request.Email.Trim().ToLower());
            var user = users.FirstOrDefault();

            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            user.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = 900, 
                User = MapToUserProfile(user)
            };
        }

        public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new InvalidOperationException("Invalid refresh token.");
            }

            
            var users = await _unitOfWork.Users.FindAsync(u => u.RefreshToken == refreshToken && !u.IsDeleted);
            var user = users.FirstOrDefault();

            if (user == null || string.IsNullOrEmpty(user.RefreshToken))
            {
                throw new InvalidOperationException("Invalid refresh token.");
            }

            
            if (user.RefreshTokenExpiryTime == null || user.RefreshTokenExpiryTime < DateTime.UtcNow)
            {
                throw new InvalidOperationException("Refresh token has expired. Please log in again.");
            }

            var newRefreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            user.UpdatedAt = DateTime.UtcNow;

            
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            
            var newAccessToken = _tokenService.GenerateAccessToken(user);

            return new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = 900, 
                User = MapToUserProfile(user)
            };
        }

        public async Task LogoutAsync(int userId, string refreshToken)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user != null && user.RefreshToken == refreshToken)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                user.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Users.Update(user);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            
            var users = await _unitOfWork.Users.FindAsync(u => u.Email.ToLower() == request.Email.Trim().ToLower() && !u.IsDeleted);
            var user = users.FirstOrDefault();

            
            
            if (user == null)
            {
                _logger.LogWarning("Forgot password requested for non-existent email: {Email}", request.Email);
                return;
            }

            
            
            if (user.ResetTokenExpiry != null && user.ResetTokenExpiry > DateTime.UtcNow.AddMinutes(1))
            {
                _logger.LogWarning("Rate limit triggered for password reset request on email: {Email}", user.Email);
                throw new InvalidOperationException("Please wait at least 60 seconds before requesting another reset code.");
            }

            
            var resetToken = Random.Shared.Next(100000, 999999).ToString("D6");

            
            user.ResetToken = resetToken;
            user.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(2);
            user.FailedResetAttempts = 0;
            user.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            
            _logger.LogInformation("Password reset code generated for {Email}: {Code}", user.Email, resetToken);

            
            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body style=""margin:0; padding:0; background-color:#f4f7f6; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width:600px; margin:40px auto; background-color:#ffffff; border-radius:12px; box-shadow:0 4px 20px rgba(0,0,0,0.08); overflow:hidden;"">
        <tr>
            <td style=""background: linear-gradient(135deg, #2d8a4e, #4CAF50); padding:30px 40px; text-align:center;"">
                <h1 style=""color:#ffffff; margin:0; font-size:28px; font-weight:700;"">🌿 GreenCart</h1>
                <p style=""color:#e8f5e9; margin:8px 0 0 0; font-size:14px;"">Organic Herbal Supplement Store</p>
            </td>
        </tr>
        <tr>
            <td style=""padding:40px; text-align:center;"">
                <p style=""color:#4a5568; font-size:15px; line-height:1.6; margin:0 0 24px 0; text-align:left;"">
                    Hello <strong>{user.FullName}</strong>,<br><br>
                    We received a request to reset your password. Use the verification code below to reset your password on GreenCart.
                </p>
                
                <!-- 6-digit Code Display Box -->
                <div style=""margin:30px 0; padding:20px; background-color:#f0fdf4; border:2px dashed #86efac; border-radius:12px;"">
                    <span style=""font-size:36px; font-weight:800; letter-spacing:10px; color:#166534; font-family:Consolas, monospace;"">{resetToken}</span>
                </div>

                <p style=""color:#dc2626; font-size:14px; font-weight:600; margin:0 0 24px 0;"">
                    Enter this code on the password reset page. This code will expire in 2 minutes.
                </p>
                <hr style=""border:none; border-top:1px solid #e2e8f0; margin:24px 0;"">
                <p style=""color:#a0aec0; font-size:12px; line-height:1.5; margin:0; text-align:left;"">
                    If you did not request a password reset, please ignore this email. Your password will remain unchanged.
                </p>
            </td>
        </tr>
        <tr>
            <td style=""background-color:#f7fafc; padding:20px 40px; text-align:center;"">
                <p style=""color:#a0aec0; font-size:12px; margin:0;"">© {DateTime.UtcNow.Year} GreenCart. All rights reserved.</p>
            </td>
        </tr>
    </table>
</body>
</html>";

            
            try
            {
                await _emailService.SendEmailAsync(
                    user.Email,
                    "Your GreenCart Password Reset Code",
                    htmlBody);

                _logger.LogInformation("Password reset email sent to {Email}.", user.Email);
            }
            catch (Exception ex)
            {
                
                _logger.LogError(ex, "Failed to send password reset email to {Email}. Code was saved but email delivery failed.", user.Email);
            }
        }

        public async Task<VerifyResetCodeResponse> VerifyResetCodeAsync(VerifyResetCodeRequest request)
        {
            var users = await _unitOfWork.Users.FindAsync(u => u.Email.ToLower() == request.Email.Trim().ToLower() && !u.IsDeleted);
            var user = users.FirstOrDefault();

            if (user == null || string.IsNullOrEmpty(user.ResetToken))
            {
                return new VerifyResetCodeResponse
                {
                    IsValid = false,
                    Message = "Invalid or expired code. Please request a new one."
                };
            }

            if (user.ResetTokenExpiry == null || user.ResetTokenExpiry < DateTime.UtcNow)
            {
                return new VerifyResetCodeResponse
                {
                    IsValid = false,
                    Message = "Reset code has expired. Please request a new one."
                };
            }

            if (user.ResetToken != request.Code.Trim())
            {
                return new VerifyResetCodeResponse
                {
                    IsValid = false,
                    Message = "Invalid verification code. Please check your email and try again."
                };
            }

            return new VerifyResetCodeResponse
            {
                IsValid = true,
                Message = "Code verified. Please enter your new password."
            };
        }

        public async Task ResetPasswordAsync(ResetPasswordRequest request)
        {
            
            var users = await _unitOfWork.Users.FindAsync(u => u.Email.ToLower() == request.Email.Trim().ToLower() && !u.IsDeleted);
            var user = users.FirstOrDefault();

            if (user == null || string.IsNullOrEmpty(user.ResetToken))
            {
                throw new InvalidOperationException("Invalid or expired reset token.");
            }

            
            if (user.ResetTokenExpiry == null || user.ResetTokenExpiry < DateTime.UtcNow)
            {
                
                user.ResetToken = null;
                user.ResetTokenExpiry = null;
                user.FailedResetAttempts = 0;
                user.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Users.Update(user);
                await _unitOfWork.SaveChangesAsync();

                throw new InvalidOperationException("Reset code has expired. Please request a new code.");
            }

            if (user.ResetToken != request.Token.Trim())
            {
                user.FailedResetAttempts++;

                if (user.FailedResetAttempts >= 5)
                {
                    user.ResetToken = null;
                    user.ResetTokenExpiry = null;
                    user.FailedResetAttempts = 0;
                    user.UpdatedAt = DateTime.UtcNow;

                    _unitOfWork.Users.Update(user);
                    await _unitOfWork.SaveChangesAsync();

                    _logger.LogWarning("User {Email} exceeded 5 failed OTP attempts. OTP invalidated.", user.Email);
                    throw new InvalidOperationException("Too many failed attempts. Your reset code has been invalidated. Please request a new code.");
                }

                user.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Users.Update(user);
                await _unitOfWork.SaveChangesAsync();

                var attemptsLeft = 5 - user.FailedResetAttempts;
                throw new InvalidOperationException($"Invalid reset code. You have {attemptsLeft} attempt(s) remaining.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            user.ResetToken = null;
            user.ResetTokenExpiry = null;
            user.FailedResetAttempts = 0;
            user.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Password successfully reset for user {Email}.", user.Email);
        }

        public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Current password is incorrect.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<UserProfileResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            if (!string.IsNullOrWhiteSpace(request.FullName))
                user.FullName = request.FullName.Trim();

            if (request.PhoneNumber != null)
                user.PhoneNumber = request.PhoneNumber.Trim();

            if (request.Address != null)
                user.Address = request.Address.Trim();

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return MapToUserProfile(user);
        }

        public async Task<UserProfileResponse> GetCurrentUserAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            return MapToUserProfile(user);
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
