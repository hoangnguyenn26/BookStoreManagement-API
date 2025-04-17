using Bookstore.Application.Dtos.Books;

namespace Bookstore.Application.Interfaces.Services
{
    public interface IBookService
    {
        Task<IEnumerable<BookDto>> GetAllBooksAsync(CancellationToken cancellationToken = default);
        Task<BookDto?> GetBookByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<BookDto> CreateBookAsync(CreateBookDto createBookDto, CancellationToken cancellationToken = default);
        Task<bool> UpdateBookAsync(Guid id, UpdateBookDto updateBookDto, CancellationToken cancellationToken = default);
        Task<bool> DeleteBookAsync(Guid id, CancellationToken cancellationToken = default);

    }
}