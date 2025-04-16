
using Bookstore.Domain.Entities;

namespace Bookstore.Domain.Interfaces.Repositories
{
    public interface IOrderShippingAddressRepository
    {
        Task<OrderShippingAddress> AddAsync(OrderShippingAddress address, CancellationToken cancellationToken = default);
    }
}