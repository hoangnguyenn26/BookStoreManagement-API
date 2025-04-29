using Bookstore.Application.Dtos.Inventory;

namespace Bookstore.Application.Interfaces.Services
{
    public interface IInventoryService
    {
        // Điều chỉnh tồn kho thủ công
        Task<int> AdjustStockManuallyAsync(Guid userId, AdjustInventoryRequestDto adjustDto, CancellationToken cancellationToken = default);
    }
}