
namespace Bookstore.Application.Dtos.Books
{
    public class UpdateBookDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? ISBN { get; set; }
        public Guid? AuthorId { get; set; }
        public string? Publisher { get; set; }
        public int? PublicationYear { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public Guid CategoryId { get; set; }
    }
}