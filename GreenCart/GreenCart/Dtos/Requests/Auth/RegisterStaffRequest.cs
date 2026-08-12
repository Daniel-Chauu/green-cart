using System.ComponentModel.DataAnnotations;
using GreenCart.Entities.Enums;

namespace GreenCart.Dtos.Requests.Auth
{
    public class RegisterStaffRequest : RegisterRequest
    {
        [Required]
        public UserRole Role { get; set; } // Must be Staff or Admin
    }
}
