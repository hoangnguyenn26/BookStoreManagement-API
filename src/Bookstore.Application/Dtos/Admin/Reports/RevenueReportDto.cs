
namespace Bookstore.Application.Dtos.Admin.Reports
{
    public class RevenueReportItemDto
    {
        public DateTime Date { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class RevenueReportDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal GrandTotalRevenue { get; set; }
        public int GrandTotalOrders { get; set; }
        public List<RevenueReportItemDto> DailyRevenue { get; set; } = new List<RevenueReportItemDto>(); // Danh sách doanh thu theo ngày
    }
}