
namespace Bookstore.Application.Dtos.Reviews
{
    public class ReviewDto
    {
        public Guid Id { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string UserName { get; set; } = null!;
        public bool IsApproved { get; set; }
    }
}