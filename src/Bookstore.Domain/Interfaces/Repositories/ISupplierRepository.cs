
using Bookstore.Domain.Entities;
namespace Bookstore.Domain.Interfaces.Repositories
{
    public interface ISupplierRepository : IGenericRepository<Supplier>
    {
        // Task<Supplier?> GetByNameAsync(string name, ...); // Ví dụ nếu cần sau này
    }
}