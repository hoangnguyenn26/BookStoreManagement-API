using Bookstore.Application.Dtos.Addresses;
using FluentValidation;

public class UpdateAddressDtoValidator : AbstractValidator<CreateAddressDto>
{
    public UpdateAddressDtoValidator()
    {
        RuleFor(x => x.Street).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Village).NotEmpty().MaximumLength(100);
        RuleFor(x => x.District).NotEmpty().MaximumLength(100);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
    }
}