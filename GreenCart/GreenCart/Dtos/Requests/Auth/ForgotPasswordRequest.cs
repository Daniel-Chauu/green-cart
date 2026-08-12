using System.ComponentModel.DataAnnotations;

namespace GreenCart.Dtos.Requests.Auth
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
