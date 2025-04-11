using Bookstore.Domain.Entities;
using Bookstore.Domain.Interfaces.Repositories;
using Bookstore.Infrastructure.Persistence;


namespace Bookstore.Infrastructure.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {

        }
    }
}
