
using AutoMapper;
using Bookstore.Application.Dtos;
using Bookstore.Application.Dtos.Addresses;
using Bookstore.Application.Dtos.Admin.Reports;
using Bookstore.Application.Dtos.Authors;
using Bookstore.Application.Dtos.Books;
using Bookstore.Application.Dtos.Carts;
using Bookstore.Application.Dtos.Categories;
using Bookstore.Application.Dtos.Dashboard;
using Bookstore.Application.Dtos.Orders;
using Bookstore.Application.Dtos.Promotions;
using Bookstore.Application.Dtos.Reviews;
using Bookstore.Application.Dtos.StockReceipts;
using Bookstore.Application.Dtos.Suppliers;
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
            CreateMap<Category, Dtos.Categories.CategorySummaryDto>();

            // ----- Author Mappings -----
            CreateMap<Author, AuthorSummaryDto>();

            // ----- Book Mappings -----
            CreateMap<Book, BookDto>();
            CreateMap<CreateBookDto, Book>();
            CreateMap<UpdateBookDto, Book>();

            // ----- User Mappings -----
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src =>
                    src.UserRoles != null
                    ? src.UserRoles.Where(ur => ur.Role != null)
                                   .Select(ur => ur.Role!.Name)
                                   .ToList()
                    : new List<string>()
                ));

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

            // ----- Review Mappings -----
            CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : "Unknown User"));
            CreateMap<CreateReviewDto, Review>();

            // ----- Dashboard Mappings -----
            CreateMap<Book, BookSummaryDto>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author != null ? src.Author.Name : null));
            CreateMap<Promotion, PromotionSummaryDto>();
            CreateMap<Category, Dtos.Dashboard.CategorySummaryDto>();


            // ----- Report Mappings -----
            CreateMap<Book, LowStockBookDto>()
                 .ForMember(dest => dest.BookId, opt => opt.MapFrom(src => src.Id))
                 .ForMember(dest => dest.BookTitle, opt => opt.MapFrom(src => src.Title))
                 .ForMember(dest => dest.CurrentStockQuantity, opt => opt.MapFrom(src => src.StockQuantity))
                 .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author != null ? src.Author.Name : null));

            // ----- Supplier Mappings -----
            CreateMap<Supplier, SupplierDto>();
            CreateMap<CreateSupplierDto, Supplier>();
            CreateMap<UpdateSupplierDto, Supplier>();

            // ----- Stock Receipt Mappings -----
            // ----- Stock Receipt Mappings -----
            CreateMap<StockReceipt, StockReceiptDto>()
                .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.StockReceiptDetails));

            CreateMap<CreateStockReceiptDto, StockReceipt>();
            CreateMap<CreateStockReceiptDetailDto, StockReceiptDetail>();

            CreateMap<StockReceiptDetail, StockReceiptDetailDto>()
                 .ForMember(dest => dest.BookTitle, opt => opt.MapFrom(src => src.Book != null ? src.Book.Title : null));


        }
    }
}