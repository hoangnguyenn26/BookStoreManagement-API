
using Bookstore.Domain.Entities;

namespace Bookstore.Domain.Interfaces.Repositories
{
    public interface IAddressRepository : IGenericRepository<Address>
    {
        Task<IEnumerable<Address>> GetAddressesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<Address?> GetDefaultAddressByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task UnsetDefaultAddressesAsync(Guid userId, Guid? exceptAddressId = null, CancellationToken cancellationToken = default);
    }
}