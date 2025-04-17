
using AutoMapper;
using Bookstore.Application.Dtos.Promotions;
using Bookstore.Application.Interfaces;
using Bookstore.Application.Interfaces.Services;
using Bookstore.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Bookstore.Application.Services
{
    public class PromotionService : IPromotionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PromotionService> _logger;

        public PromotionService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PromotionService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PromotionDto> CreatePromotionAsync(CreatePromotionDto createDto, CancellationToken cancellationToken = default)
        {
            var existingPromo = await _unitOfWork.PromotionRepository.GetByCodeAsync(createDto.Code, cancellationToken);
            if (existingPromo != null)
            {
                throw new ValidationException($"Promotion code '{createDto.Code}' already exists.");
            }

            if (createDto.DiscountPercentage.HasValue && createDto.DiscountAmount.HasValue)
            {
                throw new ValidationException("Promotion cannot have both percentage and fixed amount discount.");
            }
            if (!createDto.DiscountPercentage.HasValue && !createDto.DiscountAmount.HasValue)
            {
                throw new ValidationException("Promotion must have either a percentage or a fixed amount discount.");
            }


            var promotionEntity = _mapper.Map<Promotion>(createDto);
            promotionEntity.CurrentUsage = 0; // Luôn bắt đầu từ 0

            var createdPromo = await _unitOfWork.PromotionRepository.AddAsync(promotionEntity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<PromotionDto>(createdPromo);
        }

        public async Task<bool> DeletePromotionAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var promo = await _unitOfWork.PromotionRepository.GetByIdAsync(id, cancellationToken);
            if (promo == null) return false;

            // Có thể dùng soft delete nếu Promotion có cờ IsDeleted
            await _unitOfWork.PromotionRepository.DeleteAsync(promo, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<IEnumerable<PromotionDto>> GetActivePromotionsForDisplayAsync(CancellationToken cancellationToken = default)
        {
            var activePromos = await _unitOfWork.PromotionRepository.GetActivePromotionsAsync(DateTime.UtcNow, cancellationToken);
            return _mapper.Map<IEnumerable<PromotionDto>>(activePromos);
        }


        public async Task<IEnumerable<PromotionDto>> GetAllPromotionsAsync(CancellationToken cancellationToken = default)
        {
            var promotions = await _unitOfWork.PromotionRepository.ListAsync(orderBy: q => q.OrderByDescending(p => p.CreatedAtUtc), cancellationToken: cancellationToken);
            return _mapper.Map<IEnumerable<PromotionDto>>(promotions);
        }

        public async Task<PromotionDto?> GetPromotionByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            var promo = await _unitOfWork.PromotionRepository.GetByCodeAsync(code, cancellationToken);
            return promo == null ? null : _mapper.Map<PromotionDto>(promo);
        }

        public async Task<PromotionDto?> GetPromotionByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var promo = await _unitOfWork.PromotionRepository.GetByIdAsync(id, cancellationToken);
            return promo == null ? null : _mapper.Map<PromotionDto>(promo);
        }

        public async Task IncrementPromotionUsageAsync(string code, CancellationToken cancellationToken = default)
        {
            var promo = await _unitOfWork.PromotionRepository.GetByCodeAsync(code, cancellationToken);
            if (promo != null)
            {
                promo.CurrentUsage += 1;
                await _unitOfWork.PromotionRepository.UpdateAsync(promo, cancellationToken);
                _logger.LogInformation("Incremented usage count for promotion code {PromotionCode}", code);
            }
            else
            {
                _logger.LogWarning("Attempted to increment usage for non-existent promotion code {PromotionCode}", code);
            }
        }

        public async Task<bool> UpdatePromotionAsync(Guid id, UpdatePromotionDto updateDto, CancellationToken cancellationToken = default)
        {
            var promoToUpdate = await _unitOfWork.PromotionRepository.GetByIdAsync(id, cancellationToken);
            if (promoToUpdate == null) return false;

            // Kiểm tra logic giảm giá
            if (updateDto.DiscountPercentage.HasValue && updateDto.DiscountAmount.HasValue)
            {
                throw new ValidationException("Promotion cannot have both percentage and fixed amount discount.");
            }
            if (!updateDto.DiscountPercentage.HasValue && !updateDto.DiscountAmount.HasValue)
            {
                throw new ValidationException("Promotion must have either a percentage or a fixed amount discount.");
            }

            _mapper.Map(updateDto, promoToUpdate);

            await _unitOfWork.PromotionRepository.UpdateAsync(promoToUpdate, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<decimal> ValidateAndCalculateDiscountAsync(string code, decimal currentTotal, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Validating promotion code {PromotionCode} for total {CurrentTotal}", code, currentTotal);
            var promo = await _unitOfWork.PromotionRepository.GetByCodeAsync(code, cancellationToken);

            if (promo == null)
            {
                _logger.LogWarning("Promotion code {PromotionCode} not found.", code);
                throw new ValidationException("Invalid promotion code.");
            }

            if (!promo.IsActive)
            {
                _logger.LogWarning("Promotion code {PromotionCode} is inactive.", code);
                throw new ValidationException("This promotion code is currently inactive.");
            }

            var now = DateTime.UtcNow;
            if (promo.StartDate > now)
            {
                _logger.LogWarning("Promotion code {PromotionCode} is not yet active (Starts at {StartDate}).", code, promo.StartDate);
                throw new ValidationException("This promotion code is not active yet.");
            }
            if (promo.EndDate.HasValue && promo.EndDate < now)
            {
                _logger.LogWarning("Promotion code {PromotionCode} has expired (Ended at {EndDate}).", code, promo.EndDate);
                throw new ValidationException("This promotion code has expired.");
            }

            if (promo.MaxUsage.HasValue && promo.CurrentUsage >= promo.MaxUsage.Value)
            {
                _logger.LogWarning("Promotion code {PromotionCode} has reached its usage limit ({CurrentUsage}/{MaxUsage}).", code, promo.CurrentUsage, promo.MaxUsage);
                throw new ValidationException("This promotion code has reached its usage limit.");
            }

            //Tính toán giảm giá
            decimal discountAmount = 0;
            if (promo.DiscountPercentage.HasValue)
            {
                discountAmount = currentTotal * (promo.DiscountPercentage.Value / 100m);
            }
            else if (promo.DiscountAmount.HasValue)
            {
                discountAmount = promo.DiscountAmount.Value;
            }

            // Đảm bảo giảm giá không vượt quá tổng tiền
            discountAmount = Math.Min(discountAmount, currentTotal);

            _logger.LogInformation("Promotion code {PromotionCode} validated. Discount amount: {DiscountAmount}", code, discountAmount);
            return discountAmount;
        }
    }
}