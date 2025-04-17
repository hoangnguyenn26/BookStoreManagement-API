using AutoMapper;
using Bookstore.Application.Dtos.Reviews;
using Bookstore.Application.Interfaces;
using Bookstore.Application.Interfaces.Services;
using Bookstore.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Bookstore.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ReviewService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ReviewDto> AddReviewAsync(Guid userId, Guid bookId, CreateReviewDto createDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("User {UserId} attempting to add review for Book {BookId}", userId, bookId);

            var bookExists = await _unitOfWork.BookRepository.GetByIdAsync(bookId, cancellationToken);
            if (bookExists == null || bookExists.IsDeleted)
            {
                throw new NotFoundException($"Book with Id '{bookId}' not found.");
            }

            var existingReview = await _unitOfWork.ReviewRepository.GetByUserIdAndBookIdAsync(userId, bookId, cancellationToken);
            if (existingReview != null)
            {
                throw new ValidationException("You have already reviewed this book.");
            }

            var reviewEntity = _mapper.Map<Review>(createDto);
            reviewEntity.UserId = userId;
            reviewEntity.BookId = bookId;
            reviewEntity.IsApproved = true;

            var createdReview = await _unitOfWork.ReviewRepository.AddAsync(reviewEntity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User {UserId} successfully added review {ReviewId} for Book {BookId}", userId, createdReview.Id, bookId);

            // Lấy lại kèm User để map sang DTO có UserName
            var reviewWithUser = await _unitOfWork.ReviewRepository.ListAsync(filter: r => r.Id == createdReview.Id, includeProperties: "User", cancellationToken: cancellationToken)
                                            .ContinueWith(t => t.Result.FirstOrDefault(), cancellationToken);

            return _mapper.Map<ReviewDto>(reviewWithUser);
        }

        public async Task<bool> DeleteReviewAsync(Guid reviewId, CancellationToken cancellationToken = default)
        {
            var review = await _unitOfWork.ReviewRepository.GetByIdAsync(reviewId, cancellationToken);
            if (review == null) return false;

            await _unitOfWork.ReviewRepository.DeleteAsync(review, cancellationToken); // Xóa vật lý
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Review {ReviewId} deleted by Admin.", reviewId);
            return true;
        }

        public async Task<IEnumerable<ReviewDto>> GetApprovedReviewsForBookAsync(Guid bookId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var reviews = await _unitOfWork.ReviewRepository.GetReviewsByBookIdAsync(bookId, page, pageSize, cancellationToken);
            return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

        public async Task<IEnumerable<ReviewDto>> GetAllReviewsForAdminAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            // Dùng ListAsync từ Generic Repo và Include User
            var reviews = await _unitOfWork.ReviewRepository.ListAsync(
                orderBy: q => q.OrderByDescending(r => r.CreatedAtUtc),
                includeProperties: "User,Book",
                page: page,
                pageSize: pageSize,
                cancellationToken: cancellationToken);

            return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }
    }
}