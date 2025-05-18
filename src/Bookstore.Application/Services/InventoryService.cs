using AutoMapper;
using Bookstore.Application.Dtos.Inventory;
using Bookstore.Application.Interfaces;
using Bookstore.Application.Interfaces.Services;
using Bookstore.Domain.Entities;
using Bookstore.Domain.Enums;
using LinqKit;
using Microsoft.Extensions.Logging;

namespace Bookstore.Application.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<InventoryService> _logger;

        public InventoryService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<InventoryService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<int> AdjustStockManuallyAsync(Guid userId, AdjustInventoryRequestDto adjustDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("User {UserId} attempting to adjust stock for Book {BookId}. Change: {ChangeQuantity}, Reason: {Reason}",
                userId, adjustDto.BookId, adjustDto.ChangeQuantity, adjustDto.Reason);

            // Kiểm tra Reason hợp lệ cho điều chỉnh thủ công
            if (adjustDto.Reason == InventoryReason.StockReceipt ||
                adjustDto.Reason == InventoryReason.OnlineSale ||
                adjustDto.Reason == InventoryReason.InStoreSale ||
                adjustDto.Reason == InventoryReason.OrderCancellation ||
                adjustDto.Reason == InventoryReason.InitialStock)
            {
                throw new ValidationException($"Reason '{adjustDto.Reason}' is not valid for manual inventory adjustment.");
            }
            if (adjustDto.ChangeQuantity == 0)
            {
                throw new ValidationException("Change quantity cannot be zero.");
            }


            // --- Bắt đầu Transaction ---
            using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                //Lấy sách
                var book = await _unitOfWork.BookRepository.GetByIdAsync(adjustDto.BookId, cancellationToken, isTracking: true);
                if (book == null || book.IsDeleted)
                {
                    throw new NotFoundException($"Book with Id '{adjustDto.BookId}' not found or has been deleted.");
                }

                //Tính toán số lượng mới
                int originalQuantity = book.StockQuantity;
                int newQuantity = originalQuantity + adjustDto.ChangeQuantity;

                if (newQuantity < 0)
                {
                    throw new ValidationException($"Cannot adjust stock for book '{book.Title}'. Current stock ({originalQuantity}) is less than the reduction amount ({Math.Abs(adjustDto.ChangeQuantity)}).");
                }

                book.StockQuantity = newQuantity;

                //Tạo Inventory Log
                var log = new InventoryLog
                {
                    BookId = book.Id,
                    ChangeQuantity = adjustDto.ChangeQuantity,
                    Reason = adjustDto.Reason,
                    TimestampUtc = DateTime.UtcNow,
                    OrderId = null,
                    StockReceiptId = null,
                    UserId = userId,
                };
                await _unitOfWork.InventoryLogRepository.AddAsync(log, cancellationToken);

                await _unitOfWork.CommitTransactionAsync(transaction, cancellationToken);

                _logger.LogInformation("Stock for Book {BookId} successfully adjusted by User {UserId}. New quantity: {NewQuantity}",
                    book.Id, userId, newQuantity);

                return newQuantity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adjusting stock for Book {BookId} by User {UserId}.", adjustDto.BookId, userId);
                throw;
            }
        }

        public async Task<PagedInventoryLogResult> GetInventoryHistoryAsync(
                                                    Guid? bookId = null,
                                                    InventoryReason? reason = null,
                                                    DateTime? startDate = null,
                                                    DateTime? endDate = null,
                                                    Guid? userId = null,
                                                    Guid? orderId = null,
                                                    Guid? stockReceiptId = null,
                                                    int page = 1,
                                                    int pageSize = 20,
                                                    CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching inventory history. BookId: {BookId}, Reason: {Reason}, Start: {StartDate}, End: {EndDate}, User: {UserId}, Page: {Page}",
                bookId, reason, startDate, endDate, userId, page);

            // --- Xây dựng Filter Expression ---
            var predicate = PredicateBuilder.New<InventoryLog>(true);

            if (bookId.HasValue && bookId.Value != Guid.Empty)
            {
                predicate = predicate.And(log => log.BookId == bookId.Value);
            }
            if (reason.HasValue)
            {
                predicate = predicate.And(log => log.Reason == reason.Value);
            }
            if (startDate.HasValue)
            {
                var inclusiveStartDate = startDate.Value.Date;
                predicate = predicate.And(log => log.TimestampUtc >= inclusiveStartDate);
            }
            if (endDate.HasValue)
            {
                var inclusiveEndDate = endDate.Value.Date.AddDays(1); // Đến hết ngày
                predicate = predicate.And(log => log.TimestampUtc < inclusiveEndDate);
            }
            if (userId.HasValue && userId.Value != Guid.Empty)
            {
                predicate = predicate.And(log => log.UserId == userId.Value);
            }
            if (orderId.HasValue && orderId.Value != Guid.Empty)
            {
                predicate = predicate.And(log => log.OrderId == orderId.Value);
            }
            if (stockReceiptId.HasValue && stockReceiptId.Value != Guid.Empty)
            {
                predicate = predicate.And(log => log.StockReceiptId == stockReceiptId.Value);
            }
            // --- Kết thúc Filter ---

            try
            {
                var totalCount = await _unitOfWork.InventoryLogRepository.CountAsync(predicate, cancellationToken);
                var logs = await _unitOfWork.InventoryLogRepository.GetHistoryAsync(predicate, page, pageSize, cancellationToken);

                var logDtos = _mapper.Map<IEnumerable<InventoryLogDto>>(logs);

                return new PagedInventoryLogResult
                {
                    Items = logDtos,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching inventory history.");
                throw;
            }
        }
    }
}