
using AutoMapper;
using Bookstore.Application.Dtos;
using Bookstore.Application.Dtos.Addresses;
using Bookstore.Application.Dtos.Books;
using Bookstore.Application.Dtos.Carts;
using Bookstore.Application.Dtos.Orders;
using Bookstore.Application.Dtos.Promotions;
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

            // ----- OrderShippingAddress Mapping -----
            // Map từ Address (địa chỉ gốc của user) sang OrderShippingAddress
            CreateMap<Address, OrderShippingAddress>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()); // Bỏ qua Id khi map vì sẽ tạo Id mới

            // Map từ OrderShippingAddress sang AddressDto (để hiển thị trong OrderDto)
            CreateMap<OrderShippingAddress, AddressDto>();

            // ----- Order Mappings -----
            CreateMap<OrderDetail, OrderDetailDto>();
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : "Guest"))
                .ForMember(dest => dest.ShippingAddress, opt => opt.MapFrom(src => src.OrderShippingAddress));
            CreateMap<Order, OrderSummaryDto>()
                 .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : "Guest"))
                 .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.OrderDetails.Sum(od => od.Quantity)));

            // ----- Promotion Mappings -----
            CreateMap<Promotion, PromotionDto>();
            CreateMap<CreatePromotionDto, Promotion>();
            CreateMap<UpdatePromotionDto, Promotion>();
        }
    }
}