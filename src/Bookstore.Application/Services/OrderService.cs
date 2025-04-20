using AutoMapper;
using Bookstore.Application.Dtos.Orders;
using Bookstore.Application.Exceptions;
using Bookstore.Application.Interfaces;
using Bookstore.Application.Interfaces.Services;
using Bookstore.Domain.Entities;
using Bookstore.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;


namespace Bookstore.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;
        private readonly IPromotionService _promotionService;
        public OrderService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<OrderService> logger, IPromotionService promotionService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _promotionService = promotionService;
        }

        public async Task<OrderDto> CreateOnlineOrderAsync(Guid userId, CreateOrderRequestDto createOrderDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Attempting to create online order for User: {UserId}", userId);
            var cartItems = (await _unitOfWork.CartRepository.GetCartByUserIdAsync(userId, cancellationToken)).ToList();
            if (!cartItems.Any()) throw new ValidationException("Cannot create order from an empty cart.");

            var shippingAddressEntity = await _unitOfWork.AddressRepository.GetByIdAsync(createOrderDto.ShippingAddressId, cancellationToken);
            if (shippingAddressEntity == null || shippingAddressEntity.UserId != userId) throw new NotFoundException($"Shipping address with Id '{createOrderDto.ShippingAddressId}' not found or does not belong to the user.");

            decimal subTotal = 0;
            var bookQuantities = new Dictionary<Guid, int>();

            foreach (var item in cartItems)
            {
                var book = item.Book;
                if (book == null) throw new InvalidOperationException($"Book data is missing for cart item (BookId: {item.BookId}).");
                if (item.Quantity > book.StockQuantity) throw new ValidationException($"Insufficient stock for book '{book.Title}'. Only {book.StockQuantity} available.");

                subTotal += item.Quantity * book.Price;
                bookQuantities.Add(book.Id, item.Quantity);
            }

            decimal discountAmount = 0;
            Promotion? appliedPromotion = null;
            if (!string.IsNullOrWhiteSpace(createOrderDto.PromotionCode))
            {
                try
                {
                    discountAmount = await _promotionService.ValidateAndCalculateDiscountAsync(createOrderDto.PromotionCode, subTotal, cancellationToken);
                    appliedPromotion = await _unitOfWork.PromotionRepository.GetByCodeAsync(createOrderDto.PromotionCode, cancellationToken);
                }
                catch (ValidationException ex) { throw; }
            }
            decimal finalTotalAmount = Math.Max(0, subTotal - discountAmount);

            // === TẠO ORDER VÀ XỬ LÝ THANH TOÁN ===
            using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            Order? createdOrder = null;

            try
            {
                var orderShippingAddress = _mapper.Map<OrderShippingAddress>(shippingAddressEntity);
                orderShippingAddress.Id = Guid.NewGuid();
                await _unitOfWork.OrderShippingAddressRepository.AddAsync(orderShippingAddress, cancellationToken);

                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Pending,
                    TotalAmount = finalTotalAmount,
                    OrderShippingAddress = orderShippingAddress,
                    OrderType = OrderType.Online,
                    DeliveryMethod = DeliveryMethod.Shipping,
                    PaymentMethod = PaymentMethod.OnlineGateway,
                    PaymentStatus = PaymentStatus.Pending
                };

                // Tạo OrderDetails
                order.OrderDetails = cartItems.Select(item => new OrderDetail
                {
                    BookId = item.BookId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Book.Price
                }).ToList();

                createdOrder = await _unitOfWork.OrderRepository.AddAsync(order, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken); // <<< LƯU ORDER VÀ ADDRESS TRƯỚC KHI THANH TOÁN

                _logger.LogInformation("Order {OrderId} created with Pending status for User {UserId}.", createdOrder.Id, userId);

                var (isPaymentSuccess, transactionId) = await SimulateOnlinePaymentAsync(createdOrder.Id, createdOrder.TotalAmount, cancellationToken);

                if (isPaymentSuccess)
                {
                    _logger.LogInformation("Payment successful for Order {OrderId}.", createdOrder.Id);

                    createdOrder.PaymentStatus = PaymentStatus.Completed;
                    createdOrder.Status = OrderStatus.Confirmed; // Chuyển sang Confirmed (hoặc Processing...)
                    createdOrder.TransactionId = transactionId;
                    createdOrder.InvoiceNumber = $"INV-{createdOrder.OrderDate:yyyyMMdd}-{createdOrder.Id.ToString().Substring(0, 4).ToUpper()}"; // Tạo Invoice Number đơn giản
                    await _unitOfWork.OrderRepository.UpdateAsync(createdOrder, cancellationToken);

                    var inventoryLogs = new List<InventoryLog>();
                    foreach (var bookIdAndQty in bookQuantities)
                    {
                        var book = await _unitOfWork.BookRepository.GetByIdAsync(bookIdAndQty.Key, cancellationToken);
                        if (book != null)
                        {
                            book.StockQuantity -= bookIdAndQty.Value;
                            await _unitOfWork.BookRepository.UpdateAsync(book, cancellationToken);

                            inventoryLogs.Add(new InventoryLog
                            {
                                BookId = book.Id,
                                ChangeQuantity = -bookIdAndQty.Value,
                                Reason = InventoryReason.OnlineSale,
                                TimestampUtc = DateTime.UtcNow,
                                OrderId = createdOrder.Id,
                                UserId = userId
                            });
                        }
                        else
                        {
                            _logger.LogWarning("Book {BookId} not found during stock update for Order {OrderId}", bookIdAndQty.Key, createdOrder.Id);
                        }
                    }
                    if (inventoryLogs.Any())
                    {
                        await _unitOfWork.InventoryLogRepository.AddRangeAsync(inventoryLogs, cancellationToken);
                    }

                    await _unitOfWork.CartRepository.ClearCartAsync(userId, cancellationToken);

                    if (appliedPromotion != null)
                    {
                        await _promotionService.IncrementPromotionUsageAsync(appliedPromotion.Code, cancellationToken);
                    }

                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await _unitOfWork.CommitTransactionAsync(transaction, cancellationToken);
                    _logger.LogInformation("Order {OrderId} processing completed successfully for User {UserId}.", createdOrder.Id, userId);

                }
                else
                {
                    _logger.LogWarning("Payment failed for Order {OrderId}. Setting status to Cancelled.", createdOrder.Id);
                    createdOrder.PaymentStatus = PaymentStatus.Failed;
                    createdOrder.Status = OrderStatus.Cancelled;
                    await _unitOfWork.OrderRepository.UpdateAsync(createdOrder, cancellationToken);

                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await _unitOfWork.CommitTransactionAsync(transaction, cancellationToken);

                    // Ném lỗi để Controller báo cho người dùng
                    throw new PaymentFailedException($"Online payment failed for Order {createdOrder.Id}.");
                }

                var finalOrderDetails = await _unitOfWork.OrderRepository.GetOrderWithDetailsByIdAsync(createdOrder.Id, cancellationToken);
                return _mapper.Map<OrderDto>(finalOrderDetails);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing online order for User {UserId}. Rolling back transaction if active.", userId);
                if (ex is ValidationException || ex is NotFoundException || ex is PaymentFailedException) throw;
                throw new ApplicationException("An error occurred while processing your order.", ex);
            }
        }

        public async Task<bool> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus, Guid adminUserId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Admin {AdminUserId} attempting to update status for Order {OrderId} to {NewStatus}", adminUserId, orderId, newStatus);

            var order = await _unitOfWork.OrderRepository.ListAsync(
                            filter: o => o.Id == orderId,
                            includeProperties: "OrderDetails.Book",
                            isTracking: true,
                            cancellationToken: cancellationToken)
                            .ContinueWith(t => t.Result.FirstOrDefault(), cancellationToken);


            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found for status update attempt by Admin {AdminUserId}", orderId, adminUserId);
                return false;
            }

            var originalStatus = order.Status;

            // --- Xử lý hoàn kho nếu trạng thái mới là Cancelled ---
            if (newStatus == OrderStatus.Cancelled && originalStatus != OrderStatus.Cancelled)
            {
                _logger.LogInformation("Order {OrderId} status changed to Cancelled. Processing stock replenishment.", orderId);
                await ReplenishStockAndLogAsync(order, adminUserId, InventoryReason.OrderCancellation, cancellationToken);
            }

            order.Status = newStatus;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Successfully updated status for Order {OrderId} to {NewStatus} by Admin {AdminUserId}", orderId, newStatus, adminUserId);
            return true;
        }

        public async Task<bool> CancelOrderAsync(Guid userId, Guid orderId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("User {UserId} attempting to cancel Order {OrderId}", userId, orderId);

            var order = await _unitOfWork.OrderRepository.ListAsync(
                            filter: o => o.Id == orderId,
                            includeProperties: "OrderDetails.Book",
                            isTracking: true,
                            cancellationToken: cancellationToken)
                            .ContinueWith(t => t.Result.FirstOrDefault(), cancellationToken);

            if (order == null || order.UserId != userId)
            {
                _logger.LogWarning("Order {OrderId} not found or does not belong to User {UserId} for cancellation attempt.", orderId, userId);
                return false;
            }

            // Kiểm tra trạng thái hiện tại có cho phép User hủy không (Ví dụ: chỉ Pending)
            if (order.Status != OrderStatus.Pending)
            {
                _logger.LogWarning("User {UserId} attempted to cancel Order {OrderId} which has status {Status}. Cancellation denied.", userId, orderId, order.Status);
                throw new ValidationException($"Order cannot be cancelled because its current status is '{order.Status}'.");
            }

            // --- Thực hiện hủy, hoàn kho và log ---
            await ReplenishStockAndLogAsync(order, userId, InventoryReason.OrderCancellation, cancellationToken);

            order.Status = OrderStatus.Cancelled;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("User {UserId} successfully cancelled Order {OrderId}", userId, orderId);
            return true;
        }

        // --- Hàm Helper để xử lý hoàn kho và ghi log ---
        private async Task ReplenishStockAndLogAsync(Order order, Guid actionUserId, InventoryReason reason, CancellationToken cancellationToken)
        {
            if (order.OrderDetails == null || !order.OrderDetails.Any())
            {
                _logger.LogWarning("Order {OrderId} has no details for stock replenishment.", order.Id);
                return;
            }

            var booksToUpdate = new List<Book>();
            var logsToAdd = new List<InventoryLog>();
            var timestamp = DateTime.UtcNow;

            foreach (var detail in order.OrderDetails)
            {
                var book = detail.Book;
                if (book == null)
                {
                    _logger.LogWarning("Book data missing for OrderDetail (BookId: {BookId}) in Order {OrderId} during stock replenishment.", detail.BookId, order.Id);
                    continue;
                }

                // Tăng lại số lượng tồn kho
                book.StockQuantity += detail.Quantity;
                booksToUpdate.Add(book);

                // Tạo log hoàn kho
                var log = new InventoryLog
                {
                    BookId = book.Id,
                    ChangeQuantity = detail.Quantity,
                    Reason = reason,
                    TimestampUtc = timestamp,
                    OrderId = order.Id,
                    UserId = actionUserId
                };
                logsToAdd.Add(log);
            }
            if (logsToAdd.Any())
            {
                foreach (var log in logsToAdd) { await _unitOfWork.InventoryLogRepository.AddAsync(log, cancellationToken); }
            }

        }

        public async Task<IEnumerable<OrderSummaryDto>> GetUserOrdersAsync(Guid userId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching orders for User: {UserId}, Page: {Page}, PageSize: {PageSize}", userId, page, pageSize);
            var orders = await _unitOfWork.OrderRepository.GetOrdersByUserIdAsync(userId, page, pageSize, cancellationToken);
            return _mapper.Map<IEnumerable<OrderSummaryDto>>(orders);
        }

        public async Task<OrderDto?> GetOrderByIdForUserAsync(Guid userId, Guid orderId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching order details {OrderId} for User: {UserId}", orderId, userId);
            var order = await _unitOfWork.OrderRepository.GetOrderWithDetailsByIdAsync(orderId, cancellationToken);

            // Kiểm tra đơn hàng tồn tại VÀ thuộc về đúng User
            if (order == null || order.UserId != userId)
            {
                _logger.LogWarning("Order {OrderId} not found or does not belong to User {UserId}", orderId, userId);
                return null;
            }

            return _mapper.Map<OrderDto>(order);
        }

        public async Task<IEnumerable<OrderSummaryDto>> GetAllOrdersForAdminAsync(int page = 1, int pageSize = 10, string? statusFilter = null, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching all orders for Admin. Page: {Page}, PageSize: {PageSize}, StatusFilter: {StatusFilter}", page, pageSize, statusFilter ?? "None");

            Expression<Func<Order, bool>>? filter = null;
            if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<OrderStatus>(statusFilter, true, out var status))
            {
                filter = o => o.Status == status;
            }

            var orders = await _unitOfWork.OrderRepository.GetAllOrdersAsync(filter, page, pageSize, cancellationToken);
            return _mapper.Map<IEnumerable<OrderSummaryDto>>(orders);
        }

        public async Task<OrderDto?> GetOrderByIdForAdminAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching order details {OrderId} for Admin", orderId);
            var order = await _unitOfWork.OrderRepository.GetOrderWithDetailsByIdAsync(orderId, cancellationToken);

            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found for Admin request.", orderId);
                return null;
            }
            return _mapper.Map<OrderDto>(order);
        }

        private async Task<(bool IsSuccess, string? TransactionId)> SimulateOnlinePaymentAsync(Guid orderId, decimal amount, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Simulating online payment for Order {OrderId}, Amount: {Amount}", orderId, amount);

            // --- Logic Giả lập ---
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

            bool paymentSuccess = true;

            string? transactionId = null;
            if (paymentSuccess)
            {
                transactionId = $"PMT_{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}"; // Tạo mã giao dịch giả
                _logger.LogInformation("Payment simulation successful for Order {OrderId}. TransactionId: {TransactionId}", orderId, transactionId);
                return (true, transactionId);
            }
            else
            {
                _logger.LogWarning("Payment simulation failed for Order {OrderId}.", orderId);
                return (false, null);
            }
        }
    }
}