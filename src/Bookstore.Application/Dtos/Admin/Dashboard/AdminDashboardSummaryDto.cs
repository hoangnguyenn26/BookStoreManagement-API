
namespace Bookstore.Application.Dtos.Admin.Dashboard
{
    public class AdminDashboardSummaryDto
    {
        public int NewOrdersToday { get; set; }
        public decimal TotalRevenueToday { get; set; }
        public int NewUsersToday { get; set; }
        public int LowStockItemsCount { get; set; }
    }
}