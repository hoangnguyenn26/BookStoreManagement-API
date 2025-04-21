
using System.ComponentModel.DataAnnotations;
namespace Bookstore.Application.Dtos.Admin.Users
{
    public class UpdateUserStatusDto
    {
        [Required]
        public bool IsActive { get; set; }
    }
}