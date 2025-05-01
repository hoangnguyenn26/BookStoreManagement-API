namespace Bookstore.Application.Dtos.Authors
{
    public class AuthorDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Biography { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}