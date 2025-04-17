
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
                var newestBooksEntities = await _unitOfWork.BookRepository.ListAsync(
                    orderBy: q => q.OrderByDescending(b => b.CreatedAtUtc),
                    includeProperties: "Author",
                    pageSize: 10,
                    cancellationToken: cancellationToken
                );
                dashboardDto.NewestBooks = _mapper.Map<List<BookSummaryDto>>(newestBooksEntities);

                // 2. Lấy Sách Bán Chạy (Tạm thời để trống hoặc lấy featured)
                // TODO: Implement logic lấy sách bán chạy dựa trên Orders/OrderDetails sau này
                // Ví dụ tạm: Lấy sách có rating cao (nếu có Reviews) hoặc sách ngẫu nhiên
                // var featuredBooks = await _unitOfWork.BookRepository.ListAsync(pageSize: 5, cancellationToken: cancellationToken);
                // dashboardDto.BestSellingBooks = _mapper.Map<List<BookSummaryDto>>(featuredBooks);

                var activePromotionsEntities = await _unitOfWork.PromotionRepository.GetActivePromotionsAsync(DateTime.UtcNow, cancellationToken);
                dashboardDto.ActivePromotions = _mapper.Map<List<PromotionSummaryDto>>(activePromotionsEntities);


                var featuredCategoriesEntities = await _unitOfWork.CategoryRepository.ListAsync(
                    filter: c => c.ParentCategoryId == null,
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