using System.ComponentModel.DataAnnotations;

namespace Bookstore.Application.Dtos.Authors
{
    public class CreateAuthorDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        public string? Biography { get; set; }
    }
}