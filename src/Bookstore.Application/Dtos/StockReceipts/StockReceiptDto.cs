
using Bookstore.Application.Dtos.Suppliers;

namespace Bookstore.Application.Dtos.StockReceipts
{
    public class StockReceiptDetailDto
    {
        public Guid Id { get; set; }
        public Guid BookId { get; set; }
        public string BookTitle { get; set; } = null!;
        public int QuantityReceived { get; set; }
        public decimal? PurchasePrice { get; set; }
    }

    public class StockReceiptDto
    {
        public Guid Id { get; set; }
        public Guid? SupplierId { get; set; }
        public SupplierDto? Supplier { get; set; }
        public DateTime ReceiptDate { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public List<StockReceiptDetailDto> Details { get; set; } = new List<StockReceiptDetailDto>();
    }
}