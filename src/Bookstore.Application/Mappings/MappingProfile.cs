
using AutoMapper;
using Bookstore.Application.Dtos;
using Bookstore.Application.Dtos.Addresses;
using Bookstore.Application.Dtos.Books;
using Bookstore.Application.Dtos.Carts;
using Bookstore.Application.Dtos.Orders;
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

            // ----- Address Mappings -----
            CreateMap<Address, AddressDto>();
            CreateMap<CreateAddressDto, Address>();
            CreateMap<UpdateAddressDto, Address>();
            // ----- Order Mappings -----
            CreateMap<OrderShippingAddress, AddressDto>();
            CreateMap<OrderDetail, OrderDetailDto>();
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : "Guest")) // Lấy UserName nếu User được Include
                .ForMember(dest => dest.ShippingAddress, opt => opt.MapFrom(src => src.OrderShippingAddress));

            CreateMap<Order, OrderSummaryDto>()
                 .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : "Guest"))
                 .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.OrderDetails.Sum(od => od.Quantity))); // Tính tổng số lượng item


        }
    }
}