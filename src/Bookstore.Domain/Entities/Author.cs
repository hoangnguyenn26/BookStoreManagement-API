
namespace Bookstore.Domain.Entities
{
    public class Author : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Biography { get; set; }
        public virtual ICollection<Book> Books { get; set; } = new List<Book>();
    }
}