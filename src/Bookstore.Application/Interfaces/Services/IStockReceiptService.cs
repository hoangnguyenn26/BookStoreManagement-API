
using Bookstore.Application.Dtos.StockReceipts;
namespace Bookstore.Application.Interfaces.Services
{
    public interface IStockReceiptService
    {
        Task<StockReceiptDto> CreateStockReceiptAsync(CreateStockReceiptDto createDto, Guid userId, CancellationToken cancellationToken = default);
        Task<StockReceiptDto?> GetStockReceiptByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<StockReceiptDto>> GetAllStockReceiptsAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    }
}