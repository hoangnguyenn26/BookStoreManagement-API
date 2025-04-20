
using AutoMapper;
using Bookstore.Application.Dtos.Books;
using Bookstore.Application.Interfaces;
using Bookstore.Application.Interfaces.Services;
using Bookstore.Domain.Entities;
using LinqKit;

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
                TimestampUtc = DateTime.UtcNow,
                StockReceiptId = null
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

            await _unitOfWork.BookRepository.DeleteAsync(bookToDelete, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<IEnumerable<BookDto>> GetAllBooksAsync(
    Guid? categoryId = null,
    string? search = null, // <<-- Thêm tham số vào implementation
    int page = 1,
    int pageSize = 10,
    CancellationToken cancellationToken = default)
        {
            // --- Xây dựng biểu thức lọc (Filter Expression) ---
            var predicate = PredicateBuilder.New<Book>(true);

            if (categoryId.HasValue && categoryId.Value != Guid.Empty)
            {
                var catId = categoryId.Value;
                predicate = predicate.And(b => b.CategoryId == catId);
            }
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.Trim().ToLower();
                predicate = predicate.And(b =>
                    (b.Title != null && b.Title.ToLower().Contains(searchTerm)) ||
                    (b.Author != null && b.Author.Name != null && b.Author.Name.ToLower().Contains(searchTerm)) ||
                    (b.ISBN != null && b.ISBN.ToLower().Contains(searchTerm))
                );
            }

            var books = await _unitOfWork.BookRepository.ListAsync(
                filter: predicate,
                orderBy: q => q.OrderByDescending(b => b.CreatedAtUtc),
                includeProperties: "Author,Category",
                isTracking: false,
                page: page,
                pageSize: pageSize,
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