
using AutoMapper;
using Bookstore.Application.Dtos; 
using Bookstore.Application.Dtos.Books; 
using Bookstore.Domain.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        }
    }
}