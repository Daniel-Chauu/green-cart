using GreenCart.Entities;

namespace GreenCart.Services.Common
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
    }
}
