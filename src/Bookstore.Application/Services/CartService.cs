using AutoMapper;
using Bookstore.Application.Dtos.Carts;
using Bookstore.Application.Interfaces;
using Bookstore.Application.Interfaces.Services;
using Bookstore.Domain.Entities;

namespace Bookstore.Application.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CartService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<CartItemDto?> AddOrUpdateCartItemAsync(Guid userId, AddCartItemDto addItemDto, CancellationToken cancellationToken = default)
        {
            var book = await _unitOfWork.BookRepository.GetByIdAsync(addItemDto.BookId, cancellationToken);
            if (book == null || book.IsDeleted)
            {
                throw new NotFoundException($"Book with Id '{addItemDto.BookId}' not found.");
            }
            if (addItemDto.Quantity > book.StockQuantity)
            {
                throw new ValidationException($"Insufficient stock for book '{book.Title}'. Available: {book.StockQuantity}. Requested: {addItemDto.Quantity}.");
            }
            var cartItem = await _unitOfWork.CartRepository.GetByUserIdAndBookIdAsync(userId, addItemDto.BookId, cancellationToken);

            if (cartItem == null)
            {
                // --- Thêm mới ---
                cartItem = new CartItem
                {
                    UserId = userId,
                    BookId = addItemDto.BookId,
                    Quantity = addItemDto.Quantity,
                };
                await _unitOfWork.CartRepository.AddAsync(cartItem, cancellationToken);
            }
            else
            {
                // --- Cập nhật số lượng ---
                int newQuantity = cartItem.Quantity + addItemDto.Quantity;
                if (newQuantity > book.StockQuantity)
                {
                    throw new ValidationException($"Insufficient stock for book '{book.Title}'. Available: {book.StockQuantity}. Requested total: {newQuantity}.");
                }
                cartItem.Quantity = newQuantity;
                cartItem.UpdatedAtUtc = DateTime.UtcNow;
                await _unitOfWork.CartRepository.UpdateAsync(cartItem, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            var updatedCartItem = await _unitOfWork.CartRepository.GetByUserIdAndBookIdAsync(userId, addItemDto.BookId, cancellationToken);
            if (updatedCartItem == null) return null;

            var bookInfo = await _unitOfWork.BookRepository.GetByIdAsync(updatedCartItem.BookId, cancellationToken);
            updatedCartItem.Book = bookInfo!;

            return _mapper.Map<CartItemDto>(updatedCartItem);
        }


        public async Task<bool> ClearUserCartAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.CartRepository.ClearCartAsync(userId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<IEnumerable<CartItemDto>> GetUserCartAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var cartItems = await _unitOfWork.CartRepository.GetCartByUserIdAsync(userId, cancellationToken);
            return _mapper.Map<IEnumerable<CartItemDto>>(cartItems);
        }

        public async Task<bool> RemoveCartItemAsync(Guid userId, Guid bookId, CancellationToken cancellationToken = default)
        {
            var cartItem = await _unitOfWork.CartRepository.GetByUserIdAndBookIdAsync(userId, bookId, cancellationToken);
            if (cartItem == null)
            {
                return false;
            }
            await _unitOfWork.CartRepository.DeleteAsync(cartItem, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateCartItemQuantityAsync(Guid userId, Guid bookId, UpdateCartItemDto updateDto, CancellationToken cancellationToken = default)
        {
            var cartItem = await _unitOfWork.CartRepository.GetByUserIdAndBookIdAsync(userId, bookId, cancellationToken);
            if (cartItem == null)
            {
                throw new NotFoundException($"Item with BookId '{bookId}' not found in cart.");
            }
            var book = await _unitOfWork.BookRepository.GetByIdAsync(bookId, cancellationToken);
            if (book == null || book.IsDeleted)
            {
                throw new NotFoundException($"Book with Id '{bookId}' not found.");
            }
            if (updateDto.Quantity > book.StockQuantity)
            {
                throw new ValidationException($"Insufficient stock for book '{book.Title}'. Available: {book.StockQuantity}. Requested: {updateDto.Quantity}.");
            }
            cartItem.Quantity = updateDto.Quantity;
            cartItem.UpdatedAtUtc = DateTime.UtcNow;

            await _unitOfWork.CartRepository.UpdateAsync(cartItem, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}