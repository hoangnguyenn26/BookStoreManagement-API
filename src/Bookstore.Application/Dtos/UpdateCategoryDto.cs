
namespace Bookstore.Application.Dtos
{
    public class UpdateCategoryDto
    {
        // Không bao gồm Id ở đây, Id lấy từ route
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public Guid? ParentCategoryId { get; set; }
    }
}