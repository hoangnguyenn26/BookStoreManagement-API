
using AutoMapper;
using Bookstore.Application.Dtos;
using Bookstore.Application.Dtos.Books;
using Bookstore.Application.Dtos.Carts;
using Bookstore.Application.Dtos.Wishlists;
using Bookstore.Domain.Entities;

namespace Bookstore.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ----- Category Mappings -----
            CreateMap<Category, CategoryDto>();
            CreateMap<CreateCategoryDto, Category>();
            CreateMap<UpdateCategoryDto, Category>();

            // ----- Book Mappings -----
            CreateMap<Book, BookDto>();
            CreateMap<CreateBookDto, Book>();
            CreateMap<UpdateBookDto, Book>();

            // ----- User Mappings -----
            CreateMap<User, UserDto>();
            // ----- Wishlist Mappings -----
            CreateMap<WishlistItem, WishlistItemDto>();
            // ----- Cart Mappings -----
            CreateMap<CartItem, CartItemDto>();
        }
    }
}