
using AutoMapper;
using Bookstore.Application.Dtos.Admin.Dashboard;
using Bookstore.Application.Dtos.Admin.Reports;
using Bookstore.Application.Interfaces;
using Bookstore.Application.Interfaces.Services;
using Bookstore.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Bookstore.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ReportService> _logger;

        public ReportService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ReportService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<AdminDashboardSummaryDto> GetAdminDashboardSummaryAsync(int lowStockThreshold = 5, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching admin dashboard summary.");
            var summary = new AdminDashboardSummaryDto();
            var todayStart = DateTime.UtcNow.Date;
            var tomorrowStart = todayStart.AddDays(1);

            try
            {
                // Đếm đơn hàng mới trong ngày (trạng thái không phải Cancelled)
                summary.NewOrdersToday = await _unitOfWork.OrderRepository.CountAsync(
                    filter: o => o.OrderDate >= todayStart && o.OrderDate < tomorrowStart && o.Status != OrderStatus.Cancelled,
                    cancellationToken: cancellationToken); // Giả sử có CountAsync trong Generic Repo

                // Tính tổng doanh thu trong ngày (chỉ đơn hàng Completed)
                summary.TotalRevenueToday = await _unitOfWork.OrderRepository.SumAsync(
                    filter: o => o.OrderDate >= todayStart && o.OrderDate < tomorrowStart && o.Status == OrderStatus.Completed,
                    selector: o => o.TotalAmount, // Chọn trường để tính tổng
                    cancellationToken: cancellationToken); // Giả sử có SumAsync

                // Đếm người dùng mới trong ngày
                summary.NewUsersToday = await _unitOfWork.UserRepository.CountAsync(
                    filter: u => u.CreatedAtUtc >= todayStart && u.CreatedAtUtc < tomorrowStart,
                    cancellationToken: cancellationToken);

                // Đếm sách sắp hết hàng
                summary.LowStockItemsCount = await _unitOfWork.BookRepository.CountAsync(
                    filter: b => !b.IsDeleted && b.StockQuantity > 0 && b.StockQuantity <= lowStockThreshold, // Lớn hơn 0 và nhỏ hơn hoặc bằng ngưỡng
                    cancellationToken: cancellationToken);

                _logger.LogInformation("Admin dashboard summary fetched successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching admin dashboard summary.");
            }

            return summary;
        }


        public async Task<RevenueReportDto> GetRevenueReportAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Generating revenue report from {StartDate} to {EndDate}", startDate, endDate);

            var inclusiveEndDate = endDate.Date.AddDays(1);
            var inclusiveStartDate = startDate.Date;

            var report = new RevenueReportDto
            {
                StartDate = inclusiveStartDate,
                EndDate = endDate.Date
            };

            try
            {
                var completedOrders = await _unitOfWork.OrderRepository.ListAsync(
                    filter: o => o.Status == OrderStatus.Completed &&
                                 o.OrderDate >= inclusiveStartDate &&
                                 o.OrderDate < inclusiveEndDate,
                    isTracking: false, // Chỉ đọc
                    cancellationToken: cancellationToken);

                if (completedOrders.Any())
                {
                    report.GrandTotalRevenue = completedOrders.Sum(o => o.TotalAmount);
                    report.GrandTotalOrders = completedOrders.Count;

                    // Nhóm doanh thu theo ngày
                    report.DailyRevenue = completedOrders
                        .GroupBy(o => o.OrderDate.Date)
                        .Select(g => new RevenueReportItemDto
                        {
                            Date = g.Key,
                            OrderCount = g.Count(),
                            TotalRevenue = g.Sum(o => o.TotalAmount)
                        })
                        .OrderBy(item => item.Date)
                        .ToList();
                }
                _logger.LogInformation("Revenue report generated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating revenue report from {StartDate} to {EndDate}", startDate, endDate);
            }


            return report;
        }

        public async Task<IEnumerable<BestsellerDto>> GetBestsellersReportAsync(DateTime startDate, DateTime endDate, int top = 5, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Generating bestsellers report from {StartDate} to {EndDate}, top {TopCount}", startDate, endDate, top);
            if (top <= 0) top = 5;

            try
            {
                // Gọi phương thức mới từ BookRepository thông qua UnitOfWork
                var bestsellersInfo = await _unitOfWork.BookRepository.GetBestsellersInfoAsync(startDate, endDate, top, cancellationToken);
                var bestsellersDto = _mapper.Map<IEnumerable<BestsellerDto>>(bestsellersInfo);

                _logger.LogInformation("Bestsellers report generated successfully.");
                return bestsellersDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating bestsellers report from {StartDate} to {EndDate}", startDate, endDate);
                throw;
            }
        }

        public async Task<IEnumerable<LowStockBookDto>> GetLowStockReportAsync(int lowStockThreshold = 5, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Generating low stock report with threshold {LowStockThreshold}", lowStockThreshold);
            if (lowStockThreshold < 0) lowStockThreshold = 0;

            try
            {
                var lowStockBooks = await _unitOfWork.BookRepository.ListAsync(
                    filter: b => b.StockQuantity > 0 && b.StockQuantity <= lowStockThreshold,
                    orderBy: q => q.OrderBy(b => b.StockQuantity),
                    includeProperties: "Author",
                    isTracking: false,
                    cancellationToken: cancellationToken);

                return _mapper.Map<IEnumerable<LowStockBookDto>>(lowStockBooks);
                _logger.LogInformation("Low stock report generated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating low stock report with threshold {LowStockThreshold}", lowStockThreshold);
                throw;
            }
        }
    }
}