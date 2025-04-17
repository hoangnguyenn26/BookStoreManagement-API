
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Application.Dtos.Reviews
{
    public class CreateReviewDto
    {
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }
    }
}