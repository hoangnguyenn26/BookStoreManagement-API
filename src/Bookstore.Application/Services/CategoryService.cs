using AutoMapper;
using Bookstore.Application.Dtos.Categories;
using Bookstore.Application.Interfaces;
using Bookstore.Application.Interfaces.Services;
using Bookstore.Domain.Entities;

namespace Bookstore.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto, CancellationToken cancellationToken = default)
        {
            var categoryEntity = _mapper.Map<Category>(createCategoryDto);

            var createdCategory = await _unitOfWork.CategoryRepository.AddAsync(categoryEntity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<CategoryDto>(createdCategory);
        }

        public async Task<bool> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var categoryToDelete = await _unitOfWork.CategoryRepository.GetByIdAsync(id, cancellationToken);
            if (categoryToDelete == null) return false;

            await _unitOfWork.CategoryRepository.DeleteAsync(categoryToDelete, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(string? search, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var categories = await _unitOfWork.CategoryRepository.ListAsync(
                filter: c => string.IsNullOrEmpty(search) || c.Name.Contains(search),
                orderBy: q => q.OrderBy(c => c.Name),
                isTracking: false,
                page: page,
                pageSize: pageSize,
                cancellationToken: cancellationToken);
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(id, cancellationToken);
            if (category == null || category.IsDeleted) return null;
            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<bool> UpdateCategoryAsync(Guid id, UpdateCategoryDto updateCategoryDto, CancellationToken cancellationToken = default)
        {
            var categoryToUpdate = await _unitOfWork.CategoryRepository.GetByIdAsync(id, cancellationToken);
            if (categoryToUpdate == null || categoryToUpdate.IsDeleted) return false;
            _mapper.Map(updateCategoryDto, categoryToUpdate);

            await _unitOfWork.CategoryRepository.UpdateAsync(categoryToUpdate, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
