using AutoMapper;
using Bookstore.Application.Dtos.Orders;
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

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<OrderService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OrderDto> CreateOnlineOrderAsync(Guid userId, CreateOrderRequestDto createOrderDto, CancellationToken cancellationToken = default)
        {

            // --- Bắt đầu Transaction ---
            // DbContext thường tự quản lý transaction khi SaveChanges, nhưng để chắc chắn, đặc biệt khi có nhiều thao tác, dùng transaction tường minh
            using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                // 1. Lấy giỏ hàng của User
                var cartItems = (await _unitOfWork.CartRepository.GetCartByUserIdAsync(userId, cancellationToken)).ToList();
                if (!cartItems.Any())
                {
                    _logger.LogWarning("User {UserId} attempted to create order with an empty cart.", userId);
                    throw new ValidationException("Cannot create order from an empty cart.");
                }

                // 2. Lấy địa chỉ giao hàng đã chọn của User
                var shippingAddress = await _unitOfWork.AddressRepository.GetByIdAsync(createOrderDto.ShippingAddressId, cancellationToken);
                if (shippingAddress == null || shippingAddress.UserId != userId)
                {
                    _logger.LogWarning("Invalid ShippingAddressId {AddressId} provided for User {UserId}.", createOrderDto.ShippingAddressId, userId);
                    throw new NotFoundException($"Shipping address with Id '{createOrderDto.ShippingAddressId}' not found or does not belong to the user.");
                }

                // 3. Tạo bản ghi Snapshot địa chỉ giao hàng
                var orderShippingAddress = _mapper.Map<OrderShippingAddress>(shippingAddress);
                // **Quan trọng:** Gán Id mới cho bản ghi snapshot
                orderShippingAddress.Id = Guid.NewGuid();
                await _unitOfWork.OrderShippingAddressRepository.AddAsync(orderShippingAddress, cancellationToken);

                // 4. Kiểm tra tồn kho và Tính toán ban đầu
                decimal subTotal = 0;
                var orderDetails = new List<OrderDetail>();
                var booksToUpdate = new List<Book>();

                foreach (var item in cartItems)
                {
                    var book = item.Book; // Repo đã include Book
                    if (book == null) // Kiểm tra đề phòng
                    {
                        _logger.LogError("CartItem for User {UserId} contains invalid BookId {BookId}", userId, item.BookId);
                        throw new InvalidOperationException($"Book data is missing for cart item (BookId: {item.BookId}).");
                    }

                    if (item.Quantity > book.StockQuantity)
                    {
                        _logger.LogWarning("Insufficient stock for Book {BookId} ('{BookTitle}') for User {UserId}. Requested: {RequestedQty}, Available: {AvailableQty}", book.Id, book.Title, userId, item.Quantity, book.StockQuantity);
                        throw new ValidationException($"Insufficient stock for book '{book.Title}'. Only {book.StockQuantity} available.");
                    }

                    // Tạo OrderDetail
                    var orderDetail = new OrderDetail
                    {
                        BookId = book.Id,
                        Quantity = item.Quantity,
                        UnitPrice = book.Price
                    };
                    orderDetails.Add(orderDetail);

                    // Tính tổng tiền hàng
                    subTotal += orderDetail.Quantity * orderDetail.UnitPrice;

                    // Giảm số lượng tồn kho 
                    book.StockQuantity -= item.Quantity;
                    booksToUpdate.Add(book); // Thêm vào danh sách cần cập nhật
                }

                // 5. Áp dụng Khuyến mãi
                decimal discountAmount = 0;
                // if (!string.IsNullOrWhiteSpace(createOrderDto.PromotionCode)) {
                //     discountAmount = await _promotionService.ValidateAndApplyPromotionAsync(createOrderDto.PromotionCode, subTotal);
                // }
                decimal finalTotalAmount = subTotal - discountAmount;

                // 6. Tạo bản ghi Order
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
                    PaymentStatus = PaymentStatus.Pending,
                    OrderDetails = orderDetails
                };

                await _unitOfWork.OrderRepository.AddAsync(order, cancellationToken);

                // 7. Cập nhật trạng thái (Modified) cho các sách đã thay đổi tồn kho
                foreach (var book in booksToUpdate)
                {
                    await _unitOfWork.BookRepository.UpdateAsync(book, cancellationToken);
                }


                // 8. Xóa Giỏ hàng
                await _unitOfWork.CartRepository.ClearCartAsync(userId, cancellationToken);

                // 9. Ghi Inventory Logs
                var orderIdForLog = order.Id;
                var timestampForLog = order.OrderDate;
                foreach (var detail in orderDetails)
                {
                    var log = new InventoryLog
                    {
                        BookId = detail.BookId,
                        ChangeQuantity = -detail.Quantity,
                        Reason = InventoryReason.OnlineSale,
                        TimestampUtc = timestampForLog,
                        OrderId = orderIdForLog,
                        UserId = userId
                    };
                    await _unitOfWork.InventoryLogRepository.AddAsync(log, cancellationToken);
                }

                // 10. Lưu tất cả thay đổi vào CSDL
                var changesSaved = await _unitOfWork.SaveChangesAsync(cancellationToken);
                if (changesSaved == 0)
                {
                    _logger.LogWarning("No changes were saved to the database for User {UserId} order creation.", userId);
                    throw new InvalidOperationException("Could not save the order to the database.");
                }

                // 11. Commit Transaction
                await _unitOfWork.CommitTransactionAsync(transaction, cancellationToken);

                _logger.LogInformation("Successfully created online order {OrderId} for User {UserId}", order.Id, userId);

                // 12. Lấy lại thông tin đầy đủ để trả về DTO (bao gồm cả chi tiết)
                var createdOrderWithDetails = await _unitOfWork.OrderRepository.GetOrderWithDetailsByIdAsync(order.Id, cancellationToken);
                return _mapper.Map<OrderDto>(createdOrderWithDetails);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating online order for User {UserId}", userId);
                // Rollback Transaction nếu có lỗi
                await _unitOfWork.RollbackTransactionAsync(transaction, cancellationToken);
                throw;
            }
        }

        public async Task<bool> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus, Guid adminUserId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Admin {AdminUserId} attempting to update status for Order {OrderId} to {NewStatus}", adminUserId, orderId, newStatus);

            // Lấy order kèm chi tiết để xử lý hoàn kho nếu hủy
            // **Quan trọng:** Cần bật tracking ở đây vì sẽ cập nhật Order và Book
            var order = await _unitOfWork.OrderRepository.ListAsync(
                            filter: o => o.Id == orderId,
                            includeProperties: "OrderDetails.Book", // Include đủ để hoàn kho
                            isTracking: true, // <<-- Bật Tracking
                            cancellationToken: cancellationToken)
                            .ContinueWith(t => t.Result.FirstOrDefault(), cancellationToken);


            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found for status update attempt by Admin {AdminUserId}", orderId, adminUserId);
                return false; // Hoặc throw NotFoundException
            }

            var originalStatus = order.Status;

            // --- Xử lý hoàn kho nếu trạng thái mới là Cancelled ---
            if (newStatus == OrderStatus.Cancelled && originalStatus != OrderStatus.Cancelled)
            {
                _logger.LogInformation("Order {OrderId} status changed to Cancelled. Processing stock replenishment.", orderId);
                await ReplenishStockAndLogAsync(order, adminUserId, InventoryReason.OrderCancellation, cancellationToken);
            }

            // Cập nhật trạng thái và thời gian
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
            // AutoMapper sẽ xử lý tính ItemCount nhờ cấu hình và Include trong Repo
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
                return null; // Hoặc throw NotFoundException
            }

            return _mapper.Map<OrderDto>(order);
        }

        public async Task<IEnumerable<OrderSummaryDto>> GetAllOrdersForAdminAsync(int page = 1, int pageSize = 10, string? statusFilter = null, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching all orders for Admin. Page: {Page}, PageSize: {PageSize}, StatusFilter: {StatusFilter}", page, pageSize, statusFilter ?? "None");

            // Xây dựng biểu thức lọc (filter expression) nếu có statusFilter
            Expression<Func<Order, bool>>? filter = null;
            if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<OrderStatus>(statusFilter, true, out var status)) // Thử parse string thành Enum (không phân biệt hoa thường)
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
                return null; // Hoặc throw NotFoundException
            }

            // Admin có quyền xem mọi đơn hàng nên không cần kiểm tra UserId
            return _mapper.Map<OrderDto>(order);
        }
    }
}