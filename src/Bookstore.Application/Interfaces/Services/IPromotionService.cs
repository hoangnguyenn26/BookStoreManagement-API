
using Bookstore.Application.Dtos.Promotions;

namespace Bookstore.Application.Interfaces.Services
{
    public interface IPromotionService
    {
        // CRUD cho Admin
        Task<IEnumerable<PromotionDto>> GetAllPromotionsAsync(CancellationToken cancellationToken = default);
        Task<PromotionDto?> GetPromotionByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PromotionDto?> GetPromotionByCodeAsync(string code, CancellationToken cancellationToken = default); // Cho Admin hoặc kiểm tra
        Task<PromotionDto> CreatePromotionAsync(CreatePromotionDto createDto, CancellationToken cancellationToken = default);
        Task<bool> UpdatePromotionAsync(Guid id, UpdatePromotionDto updateDto, CancellationToken cancellationToken = default);
        Task<bool> DeletePromotionAsync(Guid id, CancellationToken cancellationToken = default); // Có thể là soft delete

        // Lấy danh sách KM đang hoạt động cho User xem (Public)
        Task<IEnumerable<PromotionDto>> GetActivePromotionsForDisplayAsync(CancellationToken cancellationToken = default);

        Task<decimal> ValidateAndCalculateDiscountAsync(string code, decimal currentTotal, CancellationToken cancellationToken = default);

        // (Internal - dùng trong OrderService) Tăng lượt sử dụng sau khi Order thành công
        Task IncrementPromotionUsageAsync(string code, CancellationToken cancellationToken = default);
    }
}