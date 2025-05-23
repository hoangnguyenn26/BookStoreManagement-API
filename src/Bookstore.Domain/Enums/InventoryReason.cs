
namespace Bookstore.Domain.Enums
{
    public enum InventoryReason : byte
    {
        StockReceipt = 0,    // Nhập kho từ NCC
        OnlineSale = 1,      // Bán hàng Online
        InStoreSale = 2,     // Bán hàng tại quầy
        OrderCancellation = 3,// Hủy đơn hàng (hoàn kho)
        Return = 4,          // Khách trả hàng (hoàn kho)
        Adjustment = 5,  // Điều chỉnh Khác
        InitialStock = 6   // Tồn kho ban đầu

    }

}