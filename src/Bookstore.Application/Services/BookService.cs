
using AutoMapper;
using Bookstore.Application.Dtos.Books;
using Bookstore.Application.Interfaces; // Namespace chứa IUnitOfWork
using Bookstore.Application.Interfaces.Services;
using Bookstore.Domain.Entities;

namespace Bookstore.Application.Services
{
    public class BookService : IBookService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BookService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper;
        }

        public async Task<BookDto> CreateBookAsync(CreateBookDto createBookDto, CancellationToken cancellationToken = default)
        {
            var categoryExists = await _unitOfWork.CategoryRepository.GetByIdAsync(createBookDto.CategoryId, cancellationToken);
            if (categoryExists == null || categoryExists.IsDeleted)
            {
                throw new KeyNotFoundException($"Category with Id '{createBookDto.CategoryId}' not found.");
            }
            if (createBookDto.AuthorId.HasValue)
            {
                var authorExists = await _unitOfWork.AuthorRepository.GetByIdAsync(createBookDto.AuthorId.Value, cancellationToken);
                if (authorExists == null)
                {
                    throw new KeyNotFoundException($"Author with Id '{createBookDto.AuthorId.Value}' not found.");
                }
            }
            var bookEntity = _mapper.Map<Book>(createBookDto);

            var createdBook = await _unitOfWork.BookRepository.AddAsync(bookEntity, cancellationToken);
            var initialLog = new InventoryLog
            {
                BookId = createdBook.Id,
                ChangeQuantity = createdBook.StockQuantity,
                Reason = Domain.Enums.InventoryReason.InitialStock,
                TimestampUtc = DateTime.UtcNow
            };
            await _unitOfWork.InventoryLogRepository.AddAsync(initialLog, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<BookDto>(createdBook);
        }

        public async Task<bool> DeleteBookAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var bookToDelete = await _unitOfWork.BookRepository.GetByIdAsync(id, cancellationToken);
            if (bookToDelete == null || bookToDelete.IsDeleted)
            {
                return false;
            }

            await _unitOfWork.BookRepository.DeleteAsync(bookToDelete, cancellationToken); // Soft Delete
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<IEnumerable<BookDto>> GetAllBooksAsync(CancellationToken cancellationToken = default)
        {
            var books = await _unitOfWork.BookRepository.GetAllAsync(
                cancellationToken: cancellationToken);
            return _mapper.Map<IEnumerable<BookDto>>(books);
        }

        public async Task<BookDto?> GetBookByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var book = await _unitOfWork.BookRepository.ListAsync(
                filter: b => b.Id == id,
                isTracking: false,
                cancellationToken: cancellationToken)
                .ContinueWith(t => t.Result.FirstOrDefault(), cancellationToken);

            if (book == null)
            {
                return null;
            }
            return _mapper.Map<BookDto>(book);
        }

        public async Task<bool> UpdateBookAsync(Guid id, UpdateBookDto updateBookDto, CancellationToken cancellationToken = default)
        {
            var bookToUpdate = await _unitOfWork.BookRepository.GetByIdAsync(id, cancellationToken);
            if (bookToUpdate == null || bookToUpdate.IsDeleted)
            {
                return false;
            }

            var categoryExists = await _unitOfWork.CategoryRepository.GetByIdAsync(updateBookDto.CategoryId, cancellationToken);
            if (categoryExists == null || categoryExists.IsDeleted)
            {
                throw new KeyNotFoundException($"Category with Id '{updateBookDto.CategoryId}' not found.");
            }
            if (updateBookDto.AuthorId.HasValue)
            {
                var authorExists = await _unitOfWork.AuthorRepository.GetByIdAsync(updateBookDto.AuthorId.Value, cancellationToken);
                if (authorExists == null)
                {
                    throw new KeyNotFoundException($"Author with Id '{updateBookDto.AuthorId.Value}' not found.");
                }
            }

            int stockChange = updateBookDto.StockQuantity - bookToUpdate.StockQuantity; // Tính toán sự thay đổi tồn kho

            _mapper.Map(updateBookDto, bookToUpdate);

            await _unitOfWork.BookRepository.UpdateAsync(bookToUpdate, cancellationToken);

            // **Quan trọng:** Ghi log thay đổi tồn kho nếu có
            if (stockChange != 0)
            {
                var stockLog = new InventoryLog
                {
                    BookId = bookToUpdate.Id,
                    ChangeQuantity = stockChange,
                    Reason = Domain.Enums.InventoryReason.Adjustment,
                    TimestampUtc = DateTime.UtcNow
                };
                await _unitOfWork.InventoryLogRepository.AddAsync(stockLog, cancellationToken);
            }


            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }

    }
}