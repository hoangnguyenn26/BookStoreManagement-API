
using System.ComponentModel.DataAnnotations; // Cần cho RowVersion
using Bookstore.Domain.Interfaces;

namespace Bookstore.Domain.Entities
{
    public class Book : BaseEntity, ISoftDeleteEntity
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? ISBN { get; set; }
        public Guid? AuthorId { get; set; } // Nullable nếu tác giả có thể không xác định hoặc bị xóa
        public string? Publisher { get; set; }
        public int? PublicationYear { get; set; }
        public string? CoverImageUrl { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; } = 0;
        public Guid CategoryId { get; set; }
        public bool IsDeleted { get; set; } = false;

        [Timestamp] // For SQL Server RowVersion -> concurrency control
        public byte[] RowVersion { get; set; } = null!;
    }
}