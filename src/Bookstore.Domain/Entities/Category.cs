
using Bookstore.Domain.Interfaces;

namespace Bookstore.Domain.Entities
{
    public class Category : BaseEntity, ISoftDeleteEntity
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public Guid? ParentCategoryId { get; set; } 
        public bool IsDeleted { get; set; } = false;
        public virtual Category ParentCategory { get; set; }
        public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();
        public virtual ICollection<Book> Books { get; set; } = new List<Book>();
    }
}