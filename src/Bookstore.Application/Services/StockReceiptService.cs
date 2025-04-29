
using AutoMapper;
using Bookstore.Application.Dtos.StockReceipts;
using Bookstore.Application.Interfaces;
using Bookstore.Application.Interfaces.Services;
using Bookstore.Domain.Entities;
using Bookstore.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Bookstore.Application.Services
{
    public class StockReceiptService : IStockReceiptService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<StockReceiptService> _logger;

        public StockReceiptService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<StockReceiptService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<StockReceiptDto> CreateStockReceiptAsync(CreateStockReceiptDto createDto, Guid userId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("User {UserId} creating new stock receipt.", userId);

            using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                if (createDto.SupplierId.HasValue)
                {
                    var supplierExists = await _unitOfWork.SupplierRepository.GetByIdAsync(createDto.SupplierId.Value, cancellationToken);
                    if (supplierExists == null)
                        throw new NotFoundException($"Supplier with Id '{createDto.SupplierId.Value}' not found.");
                }

                var receiptEntity = _mapper.Map<StockReceipt>(createDto);
                await _unitOfWork.StockReceiptRepository.AddAsync(receiptEntity, cancellationToken);

                var receiptDetails = new List<StockReceiptDetail>();
                var booksToUpdate = new Dictionary<Guid, Book>();
                var inventoryLogs = new List<InventoryLog>();

                foreach (var detailDto in createDto.Details)
                {
                    // Kiểm tra BookId tồn tại
                    Book? book;
                    if (!booksToUpdate.TryGetValue(detailDto.BookId, out book))
                    {
                        book = await _unitOfWork.BookRepository.GetByIdAsync(detailDto.BookId, cancellationToken, isTracking: true);
                        if (book == null || book.IsDeleted)
                            throw new NotFoundException($"Book with Id '{detailDto.BookId}' not found or has been deleted.");
                        booksToUpdate.Add(book.Id, book);
                    }


                    // Tạo StockReceiptDetail Entity
                    var detailEntity = _mapper.Map<StockReceiptDetail>(detailDto);
                    detailEntity.StockReceiptId = receiptEntity.Id;
                    receiptDetails.Add(detailEntity);

                    book.StockQuantity += detailDto.QuantityReceived;

                    inventoryLogs.Add(new InventoryLog
                    {
                        BookId = book.Id,
                        ChangeQuantity = detailDto.QuantityReceived,
                        Reason = InventoryReason.StockReceipt,
                        TimestampUtc = receiptEntity.ReceiptDate,
                        StockReceiptId = receiptEntity.Id,
                        UserId = userId
                    });
                }

                await _unitOfWork.StockReceiptDetailRepository.AddRangeAsync(receiptDetails, cancellationToken);
                await _unitOfWork.InventoryLogRepository.AddRangeAsync(inventoryLogs, cancellationToken);

                await _unitOfWork.CommitTransactionAsync(transaction, cancellationToken);

                _logger.LogInformation("Stock receipt {ReceiptId} created successfully by User {UserId}.", receiptEntity.Id, userId);

                var createdReceiptWithDetails = await _unitOfWork.StockReceiptRepository.GetReceiptWithDetailsByIdAsync(receiptEntity.Id, cancellationToken: cancellationToken);
                if (createdReceiptWithDetails == null) throw new InvalidOperationException("Failed to retrieve the created stock receipt details.");
                return _mapper.Map<StockReceiptDto>(createdReceiptWithDetails);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating stock receipt for User {UserId}.", userId);
                throw;
            }
        }

        public async Task<IEnumerable<StockReceiptDto>> GetAllStockReceiptsAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching all stock receipts. Page: {Page}, PageSize: {PageSize}", page, pageSize);
            var receipts = await _unitOfWork.StockReceiptRepository.GetAllReceiptsAsync(page, pageSize, cancellationToken: cancellationToken);
            return _mapper.Map<IEnumerable<StockReceiptDto>>(receipts);
        }

        public async Task<StockReceiptDto?> GetStockReceiptByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching stock receipt with Id {ReceiptId}", id);
            var receipt = await _unitOfWork.StockReceiptRepository.GetReceiptWithDetailsByIdAsync(id, cancellationToken: cancellationToken);
            if (receipt == null)
            {
                _logger.LogWarning("Stock receipt {ReceiptId} not found.", id);
                return null;
            }
            // Repo đã include Details và Books
            return _mapper.Map<StockReceiptDto>(receipt);
        }
    }
}