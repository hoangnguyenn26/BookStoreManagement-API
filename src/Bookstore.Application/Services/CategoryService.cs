
using Bookstore.Application.Dtos;
using Bookstore.Application.Interfaces;
using Bookstore.Application.Interfaces.Services;
using Bookstore.Domain.Entities;
using Bookstore.Domain.Interfaces.Repositories; // Namespace chứa ICategoryRepository
using System;
using System.Collections.Generic;
using System.Linq; // Cần cho Select (manual mapping)
using System.Threading;
using System.Threading.Tasks;
// using Bookstore.Application.Exceptions; // Có thể tạo custom exceptions sau

namespace Bookstore.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IUnitOfWork unitOfWork /*, IMapper mapper*/)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto, CancellationToken cancellationToken = default)
        {
            var categoryEntity = new Category
            {
                Name = createCategoryDto.Name,
                Description = createCategoryDto.Description,
                ParentCategoryId = createCategoryDto.ParentCategoryId,
                IsDeleted = false // Mặc định khi tạo mới
            };
            var createdCategory = await _unitOfWork.CategoryRepository.AddAsync(categoryEntity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var categoryDto = new CategoryDto
            {
                Id = createdCategory.Id,
                Name = createdCategory.Name,
                Description = createdCategory.Description,
                ParentCategoryId = createdCategory.ParentCategoryId
            };
            return categoryDto;
        }

        public async Task<bool> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var categoryToDelete = await _unitOfWork.CategoryRepository.GetByIdAsync(id, cancellationToken);
            if (categoryToDelete == null) return false;

            await _unitOfWork.CategoryRepository.DeleteAsync(categoryToDelete, cancellationToken); // Repo xử lý soft delete
            await _unitOfWork.SaveChangesAsync(cancellationToken); // <-- Gọi SaveChanges ở đây

            return true;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
        {
            var categories = await _unitOfWork.CategoryRepository.GetAllAsync(cancellationToken: cancellationToken);
            return categories.Select(category => new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ParentCategoryId = category.ParentCategoryId
            }).ToList();
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(id, cancellationToken);
            if (category == null || category.IsDeleted) return null;
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ParentCategoryId = category.ParentCategoryId
            };
        }

        public async Task<bool> UpdateCategoryAsync(Guid id, UpdateCategoryDto updateCategoryDto, CancellationToken cancellationToken = default)
        {
            var categoryToUpdate = await _unitOfWork.CategoryRepository.GetByIdAsync(id, cancellationToken);
            if (categoryToUpdate == null || categoryToUpdate.IsDeleted)
            {
                return false; 
            }
            categoryToUpdate.Name = updateCategoryDto.Name;
            categoryToUpdate.Description = updateCategoryDto.Description;
            categoryToUpdate.ParentCategoryId = updateCategoryDto.ParentCategoryId;

            await _unitOfWork.CategoryRepository.UpdateAsync(categoryToUpdate, cancellationToken); // Chỉ đánh dấu Modified
            await _unitOfWork.SaveChangesAsync(cancellationToken); // <-- Gọi SaveChanges ở đây

            return true;
        }
    }
}
