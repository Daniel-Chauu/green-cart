using System.ComponentModel.DataAnnotations;

namespace GreenCart.Dtos.Requests.Auth
{
    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
