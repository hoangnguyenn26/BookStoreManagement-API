
using Bookstore.Application.Interfaces; 
using Bookstore.Domain.Interfaces.Repositories;
using Bookstore.Infrastructure.Persistence; 
using Bookstore.Infrastructure.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bookstore.Infrastructure.Persistence;
    public class UnitOfWork : IUnitOfWork 
    {
        private readonly ApplicationDbContext _context;
        // Sử dụng Lazy<T> để khởi tạo repo chỉ khi cần (tùy chọn)
        private Lazy<ICategoryRepository> _categoryRepository;
        private Lazy<IUserRepository> _userRepository;
        private Lazy<IBookRepository> _bookRepository;
        private Lazy<IAuthorRepository> _authorRepository;
        private Lazy<IInventoryLogRepository> _inventoryLogRepository;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

            _categoryRepository = new Lazy<ICategoryRepository>(() => new CategoryRepository(_context));
            _userRepository = new Lazy<IUserRepository>(() => new UserRepository(_context));
            _bookRepository = new Lazy<IBookRepository>(() => new BookRepository(_context));
            _authorRepository = new Lazy<IAuthorRepository>(() => new AuthorRepository(_context));
            _inventoryLogRepository = new Lazy<IInventoryLogRepository>(() => new InventoryLogRepository(_context));
        }

        public ICategoryRepository CategoryRepository => _categoryRepository.Value;
        public IUserRepository UserRepository => _userRepository.Value;
        public IBookRepository BookRepository => _bookRepository.Value;
        public IAuthorRepository AuthorRepository => _authorRepository.Value;
        public IInventoryLogRepository InventoryLogRepository => _inventoryLogRepository.Value;
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
