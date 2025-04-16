
namespace Bookstore.Domain.Entities
{
    public class StockReceiptDetail
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid StockReceiptId { get; set; }
        public Guid BookId { get; set; }
        public int? QuantityReceived { get; set; }
        public decimal? PurchasePrice { get; set; }

        // Navigation properties
        public virtual StockReceipt StockReceipt { get; set; } = null!;
        public virtual Book Book { get; set; } = null!;
    }
}