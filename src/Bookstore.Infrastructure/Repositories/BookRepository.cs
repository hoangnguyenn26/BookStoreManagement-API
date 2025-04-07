// src/Bookstore.Infrastructure/Repositories/BookRepository.cs
using Bookstore.Domain.Entities;
using Bookstore.Domain.Interfaces.Repositories;
using Bookstore.Infrastructure.Persistence;

namespace Bookstore.Infrastructure.Repositories
{
    public class BookRepository : GenericRepository<Book>, IBookRepository
    {
        public BookRepository(ApplicationDbContext context) : base(context)
        {
        }
        // Thêm các phương thức đặc thù cho Book nếu cần sau này
    }
}