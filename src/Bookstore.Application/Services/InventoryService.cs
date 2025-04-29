using AutoMapper;
using Bookstore.Application.Dtos.Inventory;
using Bookstore.Application.Interfaces;
using Bookstore.Application.Interfaces.Services;
using Bookstore.Domain.Entities;
using Bookstore.Domain.Enums;
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
    }
}