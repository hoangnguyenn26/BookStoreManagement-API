
namespace Bookstore.Domain.Enums
{
    public enum InventoryReason : byte
    {
        StockIn = 0,        // Nhập kho
        Sale = 1,           // Bán hàng
        OrderCancellation = 2,// Hủy đơn hàng (hoàn kho)
        Adjustment = 3,     // Điều chỉnh khác (hỏng, mất,...)
        InitialStock = 4    // Số lượng ban đầu (khi thêm sách mới)
    }
}