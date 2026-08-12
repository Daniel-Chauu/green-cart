using System.ComponentModel.DataAnnotations;
using GreenCart.Entities.Enums;

namespace GreenCart.Dtos.Requests.Auth
{
    public class RegisterStaffRequest : RegisterRequest
    {
        public UserRole Role { get; set; } // Must be Staff or Admin
    }
}
