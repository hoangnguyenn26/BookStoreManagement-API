using Bookstore.Application.Dtos.Addresses;
using FluentValidation;

public class CreateAddressDtoValidator : AbstractValidator<CreateAddressDto>
{
    public CreateAddressDtoValidator()
    {
        RuleFor(x => x.Street).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Village).NotEmpty().MaximumLength(100);
        RuleFor(x => x.District).NotEmpty().MaximumLength(100);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
    }
}