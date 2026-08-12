using System.ComponentModel.DataAnnotations;
using GreenCart.Entities.Enums;

namespace GreenCart.Dtos.Requests.Users
{
    public class UpdateUserRoleRequest
    {
        [Required]
        public UserRole NewRole { get; set; }
    }
}
