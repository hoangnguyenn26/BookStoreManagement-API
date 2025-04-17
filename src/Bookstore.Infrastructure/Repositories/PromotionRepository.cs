
using Bookstore.Domain.Entities;
using Bookstore.Domain.Interfaces.Repositories;
using Bookstore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Infrastructure.Repositories
{
    public class PromotionRepository : GenericRepository<Promotion>, IPromotionRepository
    {
        public PromotionRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Promotion?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.Code.ToLower() == code.ToLower(), cancellationToken);
        }

        public async Task<IEnumerable<Promotion>> GetActivePromotionsAsync(DateTime currentDate, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.IsActive == true &&
                            p.StartDate <= currentDate &&
                            (p.EndDate == null || p.EndDate >= currentDate) &&
                            (p.MaxUsage == null || p.CurrentUsage < p.MaxUsage))
                .OrderBy(p => p.EndDate)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}