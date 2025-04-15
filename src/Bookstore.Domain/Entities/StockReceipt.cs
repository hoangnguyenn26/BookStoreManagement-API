
namespace Bookstore.Domain.Entities
{
    public class StockReceipt : BaseEntity
    {
        public Guid? SupplierId { get; set; } // Cho phép NULL nếu nhập không rõ NCC
        public DateTime ReceiptDate { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }

        // Navigation properties
        public virtual Supplier? Supplier { get; set; }
        public virtual ICollection<StockReceiptDetail> StockReceiptDetails { get; set; } = new List<StockReceiptDetail>();
        // public virtual ICollection<InventoryLog> InventoryLogs { get; set; } = new List<InventoryLog>(); // Nếu cần xem log từ phiếu nhập
    }
}