using Bookstore.Application.Dtos.Inventory;
using Bookstore.Domain.Enums;

namespace Bookstore.Application.Interfaces.Services
{
    public class PagedInventoryLogResult
    {
        public IEnumerable<InventoryLogDto> Items { get; set; } = new List<InventoryLogDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }

    public interface IInventoryService
    {
        Task<int> AdjustStockManuallyAsync(Guid userId, AdjustInventoryRequestDto adjustDto, CancellationToken cancellationToken = default);
        Task<PagedInventoryLogResult> GetInventoryHistoryAsync(
            Guid? bookId = null,
            InventoryReason? reason = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            Guid? userId = null,
            Guid? orderId = null,
            Guid? stockReceiptId = null,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);
    }
}