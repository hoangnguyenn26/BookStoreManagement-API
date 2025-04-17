
namespace Bookstore.Application.Dtos.Admin.Reports
{
    public class BestsellerDto
    {
        public Guid BookId { get; set; }
        public string BookTitle { get; set; } = null!;
        public int TotalQuantitySold { get; set; }
    }
}