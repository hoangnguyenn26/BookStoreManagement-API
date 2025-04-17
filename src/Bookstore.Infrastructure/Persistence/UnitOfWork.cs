
using Bookstore.Application.Interfaces;
using Bookstore.Domain.Interfaces.Repositories;
using Bookstore.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

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
    private Lazy<IRoleRepository> _roleRepository;
    private Lazy<IWishlistRepository> _wishlistRepository;
    private Lazy<ICartRepository> _cartRepository;
    private Lazy<IAddressRepository> _addressRepository;
    private Lazy<IOrderRepository> _orderRepository;
    private Lazy<IOrderShippingAddressRepository> _orderShippingAddressRepository;
    private Lazy<IPromotionRepository> _promotionRepository;
    private Lazy<IReviewRepository> _reviewRepository;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));

        _categoryRepository = new Lazy<ICategoryRepository>(() => new CategoryRepository(_context));
        _userRepository = new Lazy<IUserRepository>(() => new UserRepository(_context));
        _bookRepository = new Lazy<IBookRepository>(() => new BookRepository(_context));
        _authorRepository = new Lazy<IAuthorRepository>(() => new AuthorRepository(_context));
        _inventoryLogRepository = new Lazy<IInventoryLogRepository>(() => new InventoryLogRepository(_context));
        _roleRepository = new Lazy<IRoleRepository>(() => new RoleRepository(_context));
        _wishlistRepository = new Lazy<IWishlistRepository>(() => new WishlistRepository(_context));
        _cartRepository = new Lazy<ICartRepository>(() => new CartRepository(_context));
        _addressRepository = new Lazy<IAddressRepository>(() => new AddressRepository(_context));
        _orderRepository = new Lazy<IOrderRepository>(() => new OrderRepository(_context));
        _orderShippingAddressRepository = new Lazy<IOrderShippingAddressRepository>(() => new OrderShippingAddressRepository(_context));
        _promotionRepository = new Lazy<IPromotionRepository>(() => new PromotionRepository(_context));
        _reviewRepository = new Lazy<IReviewRepository>(() => new ReviewRepository(_context));
    }

    public ICategoryRepository CategoryRepository => _categoryRepository.Value;
    public IUserRepository UserRepository => _userRepository.Value;
    public IBookRepository BookRepository => _bookRepository.Value;
    public IAuthorRepository AuthorRepository => _authorRepository.Value;
    public IInventoryLogRepository InventoryLogRepository => _inventoryLogRepository.Value;
    public IRoleRepository RoleRepository => _roleRepository.Value;
    public IWishlistRepository WishlistRepository => _wishlistRepository.Value;
    public ICartRepository CartRepository => _cartRepository.Value;
    public IAddressRepository AddressRepository => _addressRepository.Value;
    public IOrderRepository OrderRepository => _orderRepository.Value;
    public IOrderShippingAddressRepository OrderShippingAddressRepository => _orderShippingAddressRepository.Value;
    public IPromotionRepository PromotionRepository => _promotionRepository.Value;
    public IReviewRepository ReviewRepository => _reviewRepository.Value;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default)
    {
        if (transaction == null) throw new ArgumentNullException(nameof(transaction));

        try
        {
            await SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(transaction, cancellationToken);
            throw;
        }
    }

    public async Task RollbackTransactionAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default)
    {
        if (transaction == null) throw new ArgumentNullException(nameof(transaction));

        try
        {
            await transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            // Không cần Dispose transaction ở đây, khối using trong Service sẽ làm điều đó
        }
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
