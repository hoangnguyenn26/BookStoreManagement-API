
using Bookstore.Application.Dtos.Books;
using Bookstore.Application.Interfaces; // Namespace chứa IUnitOfWork
using Bookstore.Application.Interfaces.Services;
using Bookstore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bookstore.Application.Services
{
    public class BookService : IBookService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BookService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
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
            var bookEntity = new Book
            {
                Title = createBookDto.Title,
                Description = createBookDto.Description,
                ISBN = createBookDto.ISBN,
                AuthorId = createBookDto.AuthorId,
                Publisher = createBookDto.Publisher,
                PublicationYear = createBookDto.PublicationYear,
                Price = createBookDto.Price,
                StockQuantity = createBookDto.StockQuantity >= 0 ? createBookDto.StockQuantity : 0, // Đảm bảo không âm
                CategoryId = createBookDto.CategoryId,
                IsDeleted = false
            };

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

            var bookDto = MapBookToDto(createdBook); 
            return bookDto;
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
            return books.Select(MapBookToDto).ToList();
        }

        public async Task<BookDto?> GetBookByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var book = await _unitOfWork.BookRepository.ListAsync(
                filter: b => b.Id == id,
                isTracking: false, 
                cancellationToken: cancellationToken)
                .ContinueWith(t => t.Result.FirstOrDefault(), cancellationToken); // Lấy phần tử đầu tiên

            if (book == null) 
            {
                return null;
            }

            // --- Mapping thủ công ---
            return MapBookToDto(book);
        }

        public async Task<bool> UpdateBookAsync(Guid id, UpdateBookDto updateBookDto, CancellationToken cancellationToken = default)
        {
            var bookToUpdate = await _unitOfWork.BookRepository.GetByIdAsync(id, cancellationToken);
            if (bookToUpdate == null || bookToUpdate.IsDeleted)
            {
                return false;
            }

            // --- (Optional) Kiểm tra logic nghiệp vụ ---
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

            // --- Mapping thủ công DTO -> Entity đã tải ---
            int stockChange = updateBookDto.StockQuantity - bookToUpdate.StockQuantity; // Tính toán sự thay đổi tồn kho

            bookToUpdate.Title = updateBookDto.Title;
            bookToUpdate.Description = updateBookDto.Description;
            bookToUpdate.ISBN = updateBookDto.ISBN;
            bookToUpdate.AuthorId = updateBookDto.AuthorId;
            bookToUpdate.Publisher = updateBookDto.Publisher;
            bookToUpdate.PublicationYear = updateBookDto.PublicationYear;
            bookToUpdate.Price = updateBookDto.Price;
            bookToUpdate.StockQuantity = updateBookDto.StockQuantity >= 0 ? updateBookDto.StockQuantity : 0;
            bookToUpdate.CategoryId = updateBookDto.CategoryId;
            // UpdatedAtUtc tự động cập nhật

            await _unitOfWork.BookRepository.UpdateAsync(bookToUpdate, cancellationToken);

            // **Quan trọng:** Ghi log thay đổi tồn kho nếu có
            if (stockChange != 0)
            {
                var stockLog = new InventoryLog
                {
                    BookId = bookToUpdate.Id,
                    ChangeQuantity = stockChange,
                    // Lý do cần xác định rõ hơn, ví dụ: điều chỉnh thủ công
                    Reason = Domain.Enums.InventoryReason.Adjustment,
                    TimestampUtc = DateTime.UtcNow
                    // UserId của Admin thực hiện
                };
                await _unitOfWork.InventoryLogRepository.AddAsync(stockLog, cancellationToken); // Giả sử có IInventoryLogRepository
            }


            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }

        // Helper function for mapping (để tránh lặp code)
        private static BookDto MapBookToDto(Book book)
        {
            return new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Description = book.Description,
                ISBN = book.ISBN,
                AuthorId = book.AuthorId,
                // Author = book.Author != null ? new AuthorDto { Id = book.Author.Id, Name = book.Author.Name } : null, // Nếu include và có AuthorDto
                Publisher = book.Publisher,
                PublicationYear = book.PublicationYear,
                CoverImageUrl = book.CoverImageUrl,
                Price = book.Price,
                StockQuantity = book.StockQuantity,
                CategoryId = book.CategoryId,
                // Category = book.Category != null ? new CategoryDto { Id = book.Category.Id, Name = book.Category.Name } : null // Nếu include và có CategoryDto
            };
        }
    }
}