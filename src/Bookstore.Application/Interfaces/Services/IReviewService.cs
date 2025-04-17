using Bookstore.Application.Dtos.Reviews;

namespace Bookstore.Application.Interfaces.Services
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewDto>> GetApprovedReviewsForBookAsync(Guid bookId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<ReviewDto> AddReviewAsync(Guid userId, Guid bookId, CreateReviewDto createDto, CancellationToken cancellationToken = default);
        // Cho admin
        Task<bool> DeleteReviewAsync(Guid reviewId, CancellationToken cancellationToken = default);
        Task<IEnumerable<ReviewDto>> GetAllReviewsForAdminAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    }
}