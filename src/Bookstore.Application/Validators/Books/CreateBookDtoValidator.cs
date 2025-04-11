
using Bookstore.Application.Dtos.Books;
using FluentValidation;

namespace Bookstore.Application.Validators.Books
{
    public class CreateBookDtoValidator : AbstractValidator<CreateBookDto>
    {
        public CreateBookDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Book title is required.")
                .MaximumLength(256).WithMessage("Title cannot exceed 256 characters.");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price must be non-negative.");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Stock quantity must be non-negative.");

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("Category is required.");

            RuleFor(x => x.PublicationYear)
                .LessThanOrEqualTo(DateTime.UtcNow.Year + 1).When(x => x.PublicationYear.HasValue)
                .WithMessage("Publication year is invalid.");

            RuleFor(x => x.ISBN)
                .MaximumLength(20).WithMessage("ISBN cannot exceed 20 characters.");
            RuleFor(x => x.Publisher)
                .MaximumLength(100).WithMessage("Publisher cannot exceed 100 characters.");

        }
    }
}