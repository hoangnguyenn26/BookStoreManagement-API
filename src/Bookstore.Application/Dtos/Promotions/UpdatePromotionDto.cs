
namespace Bookstore.Application.Dtos.Promotions
{
    public class UpdatePromotionDto
    {
        public string? Description { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public decimal? DiscountAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? MaxUsage { get; set; }
        public bool IsActive { get; set; }
    }
}