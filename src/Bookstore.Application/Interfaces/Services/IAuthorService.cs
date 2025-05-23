using Bookstore.Application.Dtos.Authors; // Namespace DTOs

namespace Bookstore.Application.Interfaces.Services
{
    public interface IAuthorService
    {
        Task<IEnumerable<AuthorDto>> GetAllAuthorsAsync(string? search, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<AuthorDto?> GetAuthorByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<AuthorDto> CreateAuthorAsync(CreateAuthorDto createDto, CancellationToken cancellationToken = default);
        Task<bool> UpdateAuthorAsync(Guid id, UpdateAuthorDto updateDto, CancellationToken cancellationToken = default);
        Task<bool> DeleteAuthorAsync(Guid id, CancellationToken cancellationToken = default);
    }
}