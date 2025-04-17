
using Bookstore.Application.Dtos.Admin.Dashboard;
using Bookstore.Application.Dtos.Admin.Reports;

namespace Bookstore.Application.Interfaces.Services
{
    public interface IReportService
    {
        Task<AdminDashboardSummaryDto> GetAdminDashboardSummaryAsync(int lowStockThreshold = 5, CancellationToken cancellationToken = default);
        Task<RevenueReportDto> GetRevenueReportAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<IEnumerable<BestsellerDto>> GetBestsellersReportAsync(DateTime startDate, DateTime endDate, int top = 5, CancellationToken cancellationToken = default);
        Task<IEnumerable<LowStockBookDto>> GetLowStockReportAsync(int lowStockThreshold = 5, CancellationToken cancellationToken = default);
    }
}