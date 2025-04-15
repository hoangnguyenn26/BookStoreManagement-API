
using Bookstore.Domain.Entities;
using Bookstore.Domain.Interfaces.Repositories;
using Bookstore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Infrastructure.Repositories
{
    public class AddressRepository : GenericRepository<Address>, IAddressRepository
    {
        public AddressRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Address>> GetAddressesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.UpdatedAtUtc)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<Address?> GetDefaultAddressByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault == true, cancellationToken);
        }

        public async Task UnsetDefaultAddressesAsync(Guid userId, Guid? exceptAddressId = null, CancellationToken cancellationToken = default)
        {
            var defaultAddresses = await _dbSet
                .Where(a => a.UserId == userId && a.IsDefault == true && (!exceptAddressId.HasValue || a.Id != exceptAddressId.Value))
                .ToListAsync(cancellationToken);

            if (defaultAddresses.Any())
            {
                foreach (var address in defaultAddresses)
                {
                    address.IsDefault = false;
                    _context.Entry(address).State = EntityState.Modified;
                }
            }
        }
    }
}