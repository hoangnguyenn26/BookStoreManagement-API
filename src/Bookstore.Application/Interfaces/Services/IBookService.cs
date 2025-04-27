using Bookstore.Application.Dtos.Books;
using Microsoft.AspNetCore.Http;

namespace Bookstore.Application.Interfaces.Services
{
    public interface IBookService
    {
        Task<IEnumerable<BookDto>> GetAllBooksAsync(
            Guid? categoryId = null,
            string? search = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);
        Task<BookDto?> GetBookByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<BookDto> CreateBookAsync(CreateBookDto createBookDto, CancellationToken cancellationToken = default);
        Task<bool> UpdateBookAsync(Guid id, UpdateBookDto updateBookDto, CancellationToken cancellationToken = default);
        Task<bool> DeleteBookAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> UpdateBookCoverImageAsync(Guid id, IFormFile imageFile, CancellationToken cancellationToken = default);
    }
}