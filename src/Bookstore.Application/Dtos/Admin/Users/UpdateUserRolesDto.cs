
using System.ComponentModel.DataAnnotations;
namespace Bookstore.Application.Dtos.Admin.Users
{
    public class UpdateUserRolesDto
    {
        [Required]
        public List<string> RoleNames { get; set; } = new List<string>();
    }
}