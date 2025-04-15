using Bookstore.Domain.Interfaces;

namespace Bookstore.Domain.Entities
{
    public class Book : BaseEntity, ISoftDeleteEntity
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? ISBN { get; set; }
        public Guid? AuthorId { get; set; }
        public string? Publisher { get; set; }
        public int? PublicationYear { get; set; }
        public string? CoverImageUrl { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; } = 0;
        public Guid CategoryId { get; set; }
        public bool IsDeleted { get; set; } = false;
        public virtual Author Author { get; set; }
        public virtual Category Category { get; set; }
    }
}