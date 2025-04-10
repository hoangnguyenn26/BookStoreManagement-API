// src/Bookstore.Application/Interfaces/Services/ICategoryService.cs
using Bookstore.Application.Dtos; // Namespace chứa DTOs
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bookstore.Application.Interfaces.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
        Task<CategoryDto?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto, CancellationToken cancellationToken = default);
        Task<bool> UpdateCategoryAsync(Guid id, UpdateCategoryDto updateCategoryDto, CancellationToken cancellationToken = default);
        Task<bool> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default); // Sử dụng Soft Delete
    }
}