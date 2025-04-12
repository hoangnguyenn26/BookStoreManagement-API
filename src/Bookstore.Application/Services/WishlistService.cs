
using AutoMapper; // Cần cho mapping
using Bookstore.Application.Dtos.Wishlists;
using Bookstore.Application.Interfaces;
using Bookstore.Application.Interfaces.Services;
using Bookstore.Domain.Entities;


namespace Bookstore.Application.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper; // Inject AutoMapper

        public WishlistService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<bool> AddToWishlistAsync(Guid userId, Guid bookId, CancellationToken cancellationToken = default)
        {
            var bookExists = await _unitOfWork.BookRepository.GetByIdAsync(bookId, cancellationToken);
            if (bookExists == null || bookExists.IsDeleted)
            {
                return false;
            }
            var existingItem = await _unitOfWork.WishlistRepository.GetByUserIdAndBookIdAsync(userId, bookId, cancellationToken);
            if (existingItem != null)
            {
                return true;
            }
            var wishlistItem = new WishlistItem
            {
                UserId = userId,
                BookId = bookId
            };

            await _unitOfWork.WishlistRepository.AddAsync(wishlistItem, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<IEnumerable<WishlistItemDto>> GetUserWishlistAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var wishlistItems = await _unitOfWork.WishlistRepository.GetWishlistByUserIdAsync(userId, cancellationToken);
            return _mapper.Map<IEnumerable<WishlistItemDto>>(wishlistItems);
        }

        public async Task<bool> RemoveFromWishlistAsync(Guid userId, Guid bookId, CancellationToken cancellationToken = default)
        {
            var itemToRemove = await _unitOfWork.WishlistRepository.GetByUserIdAndBookIdAsync(userId, bookId, cancellationToken);

            if (itemToRemove == null)
            {
                return false;
            }

            await _unitOfWork.WishlistRepository.DeleteAsync(itemToRemove, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
        public async Task<bool> RemoveFromWishlistByIdAsync(Guid userId, Guid wishlistItemId, CancellationToken cancellationToken = default)
        {
            var itemToRemove = await _unitOfWork.WishlistRepository.GetByIdAsync(wishlistItemId, cancellationToken);
            if (itemToRemove == null || itemToRemove.UserId != userId)
            {
                return false;
            }
            await _unitOfWork.WishlistRepository.DeleteAsync(itemToRemove, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}