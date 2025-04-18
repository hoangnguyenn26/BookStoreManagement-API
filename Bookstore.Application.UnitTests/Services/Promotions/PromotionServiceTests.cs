using AutoMapper;
using Bookstore.Application.Interfaces;
using Bookstore.Application.Interfaces.Services;
using Bookstore.Application.Services;
using Bookstore.Domain.Entities;
using Bookstore.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace Bookstore.Application.UnitTests.Services.Promotions
{
    public class PromotionServiceTests
    {
        // Khai báo các đối tượng mock và service cần test
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IPromotionRepository> _mockPromotionRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<PromotionService>> _mockLogger;
        private readonly IPromotionService _promotionService;

        public PromotionServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockPromotionRepository = new Mock<IPromotionRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<PromotionService>>();

            _mockUnitOfWork.Setup(uow => uow.PromotionRepository).Returns(_mockPromotionRepository.Object);

            _promotionService = new PromotionService(
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockLogger.Object
            );
        }

        // === Test Method cho ValidateAndCalculateDiscountAsync ===

        [Fact]
        public async Task ValidateAndCalculateDiscountAsync_ValidCode_ReturnsCorrectDiscount()
        {
            // Arrange
            var promotionCode = "VALID10";
            var currentTotal = 100m;
            var expectedDiscount = 10m;
            var cancellationToken = CancellationToken.None;

            var mockPromotion = new Promotion
            {
                Id = Guid.NewGuid(),
                Code = promotionCode,
                IsActive = true,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(1),
                MaxUsage = 100,
                CurrentUsage = 10,
                DiscountPercentage = 10m,
                DiscountAmount = null
            };


            _mockPromotionRepository
                .Setup(repo => repo.GetByCodeAsync(promotionCode, cancellationToken))
                .ReturnsAsync(mockPromotion);

            // Act (Thực hiện)
            var actualDiscount = await _promotionService.ValidateAndCalculateDiscountAsync(promotionCode, currentTotal, cancellationToken);

            // Assert (Kiểm chứng)
            Assert.Equal(expectedDiscount, actualDiscount);
            _mockPromotionRepository.Verify(repo => repo.GetByCodeAsync(promotionCode, cancellationToken), Times.Once);
        }

        [Fact]
        public async Task ValidateAndCalculateDiscountAsync_CodeNotFound_ThrowsValidationException()
        {
            // Arrange
            var promotionCode = "INVALIDCODE";
            var currentTotal = 100m;
            var cancellationToken = CancellationToken.None;

            // Thiết lập Mock Repository
            _mockPromotionRepository
                .Setup(repo => repo.GetByCodeAsync(promotionCode, cancellationToken))
                .ReturnsAsync((Promotion?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                _promotionService.ValidateAndCalculateDiscountAsync(promotionCode, currentTotal, cancellationToken)
            );

            Assert.Contains("Invalid promotion code", exception.Message);

            // (Optional) Verify
            _mockPromotionRepository.Verify(repo => repo.GetByCodeAsync(promotionCode, cancellationToken), Times.Once);
        }

        [Fact]
        public async Task ValidateAndCalculateDiscountAsync_InactiveCode_ThrowsValidationException()
        {
            // Arrange
            var promotionCode = "INACTIVE";
            var currentTotal = 100m;
            var cancellationToken = CancellationToken.None;
            var mockPromotion = new Promotion
            {
                Id = Guid.NewGuid(),
                Code = promotionCode,
                IsActive = false,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(1),
                MaxUsage = 100,
                CurrentUsage = 10,
                DiscountPercentage = 10m,
                DiscountAmount = null
            };

            _mockPromotionRepository
                .Setup(repo => repo.GetByCodeAsync(promotionCode, cancellationToken))
                .ReturnsAsync(mockPromotion);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() =>
               _promotionService.ValidateAndCalculateDiscountAsync(promotionCode, currentTotal, cancellationToken)
            );
        }

    }
}