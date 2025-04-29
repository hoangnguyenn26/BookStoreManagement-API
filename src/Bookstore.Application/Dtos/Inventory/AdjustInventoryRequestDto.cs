using Bookstore.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Application.Dtos.Inventory
{
    public class AdjustInventoryRequestDto
    {
        [Required]
        public Guid BookId { get; set; }

        [Required]
        public int ChangeQuantity { get; set; }

        [Required]
        // Lý do phải là một trong các loại điều chỉnh hợp lệ
        [EnumDataType(typeof(InventoryReason))]
        public InventoryReason Reason { get; set; } = InventoryReason.Adjustment; // Mặc định là Adjustment

        [MaxLength(500)]
        public string? Notes { get; set; } // Ghi chú lý do điều chỉnh
    }
}