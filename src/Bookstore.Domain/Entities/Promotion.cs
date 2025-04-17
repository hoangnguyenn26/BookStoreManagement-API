
namespace Bookstore.Domain.Entities
{
    public class Promotion : BaseEntity
    {
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public decimal? DiscountPercentage { get; set; } // vd: 10.5 cho 10.5%
        public decimal? DiscountAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? MaxUsage { get; set; }
        public int CurrentUsage { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }
}