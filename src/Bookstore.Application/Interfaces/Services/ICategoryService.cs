// src/Bookstore.Application/Interfaces/Services/ICategoryService.cs
using Bookstore.Application.Dtos.Categories; // Namespace chứa DTOs

namespace Bookstore.Application.Interfaces.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<CategoryDto?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto, CancellationToken cancellationToken = default);
        Task<bool> UpdateCategoryAsync(Guid id, UpdateCategoryDto updateCategoryDto, CancellationToken cancellationToken = default);
        Task<bool> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default); // Sử dụng Soft Delete
    }
}