using AutoMapper;
using Bookstore.Application.Dtos.Authors;
using Bookstore.Application.Interfaces;
using Bookstore.Application.Interfaces.Services;
using Bookstore.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Bookstore.Application.Services
{
    public class AuthorService : IAuthorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthorService> _logger;

        public AuthorService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<AuthorService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<AuthorDto> CreateAuthorAsync(CreateAuthorDto createDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating new author: {AuthorName}", createDto.Name);
            // Kiểm tra tên tác giả đã tồn tại chưa (không phân biệt hoa thường)
            var existingAuthor = await _unitOfWork.AuthorRepository.ListAsync(
                filter: a => a.Name.ToLower() == createDto.Name.ToLower(),
                cancellationToken: cancellationToken);

            if (existingAuthor.Any())
            {
                throw new ValidationException($"Author with name '{createDto.Name}' already exists.");
            }

            var authorEntity = _mapper.Map<Author>(createDto);
            var createdAuthor = await _unitOfWork.AuthorRepository.AddAsync(authorEntity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken); // Lưu lại ngay
            _logger.LogInformation("Author {AuthorName} created with Id {AuthorId}", createdAuthor.Name, createdAuthor.Id);
            return _mapper.Map<AuthorDto>(createdAuthor);
        }

        public async Task<bool> DeleteAuthorAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Attempting to delete author with Id {AuthorId}", id);
            var author = await _unitOfWork.AuthorRepository.GetByIdAsync(id, cancellationToken);
            if (author == null)
            {
                _logger.LogWarning("Author {AuthorId} not found for deletion.", id);
                return false;
            }

            // Kiểm tra xem tác giả có sách nào liên kết không
            var booksByAuthor = await _unitOfWork.BookRepository.ListAsync(b => b.AuthorId == id && !b.IsDeleted, cancellationToken: cancellationToken);
            if (booksByAuthor.Any())
            {
                _logger.LogWarning("Cannot delete Author {AuthorId} because they are linked to existing books.", id);
                // Hoặc có thể set AuthorId trong sách thành NULL trước khi xóa tác giả
                throw new InvalidOperationException($"Cannot delete author '{author.Name}' as they are associated with existing books. Please reassign or delete the books first.");
            }

            await _unitOfWork.AuthorRepository.DeleteAsync(author, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Author {AuthorId} deleted successfully.", id);
            return true;
        }

        public async Task<IEnumerable<AuthorDto>> GetAllAuthorsAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching all authors. Page: {Page}, PageSize: {PageSize}", page, pageSize);
            var authors = await _unitOfWork.AuthorRepository.ListAsync(
                orderBy: q => q.OrderBy(a => a.Name),
                isTracking: false,
                page: page,
                pageSize: pageSize,
                cancellationToken: cancellationToken);
            return _mapper.Map<IEnumerable<AuthorDto>>(authors);
        }

        public async Task<AuthorDto?> GetAuthorByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching author with Id {AuthorId}", id);
            var author = await _unitOfWork.AuthorRepository.GetByIdAsync(id, isTracking: false, cancellationToken: cancellationToken);
            if (author == null)
            {
                _logger.LogWarning("Author {AuthorId} not found.", id);
                return null;
            }
            return _mapper.Map<AuthorDto>(author);
        }

        public async Task<bool> UpdateAuthorAsync(Guid id, UpdateAuthorDto updateDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Attempting to update author {AuthorId}", id);
            var authorToUpdate = await _unitOfWork.AuthorRepository.GetByIdAsync(id, cancellationToken, isTracking: true);
            if (authorToUpdate == null)
            {
                _logger.LogWarning("Author {AuthorId} not found for update.", id);
                return false;
            }

            // Kiểm tra tên mới có trùng với tác giả khác không
            if (authorToUpdate.Name.ToLower() != updateDto.Name.ToLower())
            {
                var existingAuthor = await _unitOfWork.AuthorRepository.ListAsync(
                    filter: a => a.Name.ToLower() == updateDto.Name.ToLower() && a.Id != id,
                    cancellationToken: cancellationToken);
                if (existingAuthor.Any())
                {
                    throw new ValidationException($"Another author with name '{updateDto.Name}' already exists.");
                }
            }

            _mapper.Map(updateDto, authorToUpdate);

            await _unitOfWork.AuthorRepository.UpdateAsync(authorToUpdate, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Author {AuthorId} updated successfully.", id);
            return true;
        }
    }
}