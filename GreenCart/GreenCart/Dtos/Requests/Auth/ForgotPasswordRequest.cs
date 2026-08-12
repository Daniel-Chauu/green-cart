using System.ComponentModel.DataAnnotations;

namespace GreenCart.Dtos.Requests.Auth
{
    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
    }
}
