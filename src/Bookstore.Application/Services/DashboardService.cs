// src/Bookstore.Application/Services/DashboardService.cs
using AutoMapper;
using Bookstore.Application.Dtos.Dashboard;
using Bookstore.Application.Interfaces;
using Bookstore.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Bookstore.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<DashboardService> _logger;
        private const int DefaultTopBestsellers = 5;
        private const int DaysForBestsellers = 30;

        public DashboardService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<DashboardService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<HomeDashboardDto> GetHomeDashboardAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching home dashboard data.");
            var dashboardDto = new HomeDashboardDto();

            try
            {
                // --- 1. Lấy Sách Mới Nhất ---
                var newestBooksEntities = await _unitOfWork.BookRepository.ListAsync(
                     filter: b => !b.IsDeleted, // <<-- Thêm hoặc đảm bảo filter này luôn được áp dụng trong ListAsync
                     orderBy: q => q.OrderByDescending(b => b.CreatedAtUtc),
                     includeProperties: "Author",
                     pageSize: 10,
                     cancellationToken: cancellationToken
                 );
                dashboardDto.NewestBooks = _mapper.Map<List<BookSummaryDto>>(newestBooksEntities);

                // --- 2. Lấy Sách Bán Chạy Nhất (Trong 30 ngày gần nhất) ---
                var startDateBestseller = DateTime.UtcNow.AddDays(-DaysForBestsellers);
                dashboardDto.BestSellingBooks = new List<BookSummaryDto>(); // Khởi tạo rỗng
                try
                {
                    var bestsellerData = await _unitOfWork.BookRepository.GetBestsellersInfoAsync(
                        startDateBestseller,
                        DateTime.UtcNow,
                        DefaultTopBestsellers,
                        cancellationToken);

                    if (bestsellerData.Any())
                    {
                        var bestsellerBookIds = bestsellerData.Select(b => b.BookId).ToList();

                        var bestsellerBooks = await _unitOfWork.BookRepository.ListAsync(
                            filter: b => bestsellerBookIds.Contains(b.Id) && !b.IsDeleted,
                            includeProperties: "Author",
                            isTracking: false,
                            cancellationToken: cancellationToken);

                        dashboardDto.BestSellingBooks = bestsellerData
                            .Join(bestsellerBooks,
                                  data => data.BookId,
                                  book => book.Id,
                                  (data, book) => _mapper.Map<BookSummaryDto>(book))
                            .ToList();
                    }
                    _logger.LogInformation("Fetched {Count} bestselling books.", dashboardDto.BestSellingBooks.Count);
                }
                catch (Exception ex_bestseller)
                {
                    _logger.LogError(ex_bestseller, "Error fetching bestselling books.");
                }


                // --- 3. Lấy Khuyến Mãi Đang Hoạt Động ---
                var activePromotionsEntities = await _unitOfWork.PromotionRepository.GetActivePromotionsAsync(DateTime.UtcNow, cancellationToken);
                dashboardDto.ActivePromotions = _mapper.Map<List<PromotionSummaryDto>>(activePromotionsEntities);

                // --- 4. Lấy Danh Mục Nổi Bật ---
                var featuredCategoriesEntities = await _unitOfWork.CategoryRepository.ListAsync(
                    filter: c => c.ParentCategoryId == null && !c.IsDeleted, // Lấy danh mục gốc và không bị xóa
                    pageSize: 5,
                    cancellationToken: cancellationToken
                );
                dashboardDto.FeaturedCategories = _mapper.Map<List<CategorySummaryDto>>(featuredCategoriesEntities);

                _logger.LogInformation("Successfully fetched home dashboard data.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching home dashboard data.");
            }

            return dashboardDto;
        }
    }
}