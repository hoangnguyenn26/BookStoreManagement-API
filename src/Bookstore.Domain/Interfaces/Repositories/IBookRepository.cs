
using Bookstore.Domain.Entities;

namespace Bookstore.Domain.Interfaces.Repositories
{
    public interface IBookRepository : IGenericRepository<Book>
    {
        // Có thể thêm các phương thức đặc thù cho Book sau này (vd: GetBooksByCategory, GetFeaturedBooks...)
    }
}