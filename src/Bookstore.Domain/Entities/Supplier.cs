// src/Bookstore.Domain/Entities/Supplier.cs
namespace Bookstore.Domain.Entities
{
    public class Supplier : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? ContactPerson { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }

        // Navigation property (nếu cần xem các phiếu nhập của NCC này)
        //public virtual ICollection<StockReceipt> StockReceipts { get; set; } = new List<StockReceipt>();
    }
}