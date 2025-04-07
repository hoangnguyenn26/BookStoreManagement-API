
namespace Bookstore.Domain.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public Guid? ParentCategoryId { get; set; } // Nullable for top-level categories
        public bool IsDeleted { get; set; } = false;

        // Navigation properties (optional for now)
        // public virtual Category ParentCategory { get; set; }
        // public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();
        // public virtual ICollection<Book> Books { get; set; } = new List<Book>();
    }
}