namespace Bookstore.Domain.Queries
{
    public class BestsellerInfo
    {
        public Guid BookId { get; set; }
        public string BookTitle { get; set; } = null!;
        public int TotalQuantitySold { get; set; }
    }
}
