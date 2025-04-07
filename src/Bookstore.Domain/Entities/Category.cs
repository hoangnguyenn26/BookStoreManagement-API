
using Bookstore.Domain.Interfaces;

namespace Bookstore.Domain.Entities
{
    public class Category : BaseEntity, ISoftDeleteEntity
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public Guid? ParentCategoryId { get; set; } // Nullable for top-level categories
        public bool IsDeleted { get; set; } = false;

    }
}