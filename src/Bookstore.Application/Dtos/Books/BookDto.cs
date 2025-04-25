
using Bookstore.Application.Dtos.Authors;
using Bookstore.Application.Dtos.Dashboard;

namespace Bookstore.Application.Dtos.Books
{
    public class BookDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? ISBN { get; set; }
        public AuthorSummaryDto? Author { get; set; }
        public string? Publisher { get; set; }
        public int? PublicationYear { get; set; }
        public string? CoverImageUrl { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public CategorySummaryDto Category { get; set; } = null!;
    }
}