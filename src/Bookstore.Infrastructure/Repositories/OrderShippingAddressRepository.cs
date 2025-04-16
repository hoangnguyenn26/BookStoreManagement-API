
using Bookstore.Domain.Entities;
using Bookstore.Domain.Interfaces.Repositories;
using Bookstore.Infrastructure.Persistence;

namespace Bookstore.Infrastructure.Repositories
{
    public class OrderShippingAddressRepository : IOrderShippingAddressRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderShippingAddressRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<OrderShippingAddress> AddAsync(OrderShippingAddress address, CancellationToken cancellationToken = default)
        {
            await _context.OrderShippingAddresses.AddAsync(address, cancellationToken);
            return address;
        }
    }
}