// src/Bookstore.Application/Dtos/CreateCategoryDto.cs
namespace Bookstore.Application.Dtos.Categories
{
    public class CreateCategoryDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public Guid? ParentCategoryId { get; set; } // Cho phép tạo danh mục con
    }
}