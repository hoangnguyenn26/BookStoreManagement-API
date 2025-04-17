
namespace Bookstore.Application.Dtos.Admin.Reports
{
    public class LowStockBookDto
    {
        public Guid BookId { get; set; }
        public string BookTitle { get; set; } = null!;
        public int CurrentStockQuantity { get; set; }
        public string? AuthorName { get; set; }
    }
}