
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Domain.Entities
{
    public class Review : BaseEntity
    {
        public Guid BookId { get; set; }
        public Guid UserId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Comment { get; set; }
        public bool IsApproved { get; set; } = true;

        // Navigation properties
        public virtual Book Book { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}