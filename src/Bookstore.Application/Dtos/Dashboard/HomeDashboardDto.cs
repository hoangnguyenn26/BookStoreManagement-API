
namespace Bookstore.Application.Dtos.Dashboard
{
    public class HomeDashboardDto
    {
        public List<BookSummaryDto> NewestBooks { get; set; } = new List<BookSummaryDto>();
        public List<BookSummaryDto> BestSellingBooks { get; set; } = new List<BookSummaryDto>();
        public List<PromotionSummaryDto> ActivePromotions { get; set; } = new List<PromotionSummaryDto>();
        public List<CategorySummaryDto> FeaturedCategories { get; set; } = new List<CategorySummaryDto>();
    }
}