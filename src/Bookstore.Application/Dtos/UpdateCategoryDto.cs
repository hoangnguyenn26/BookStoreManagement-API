
namespace Bookstore.Application.Dtos
{
    public class UpdateCategoryDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public Guid? ParentCategoryId { get; set; }
    }
}