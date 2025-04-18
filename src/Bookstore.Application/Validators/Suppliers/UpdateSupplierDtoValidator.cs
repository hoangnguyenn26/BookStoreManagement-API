using Bookstore.Application.Dtos.Suppliers;
using FluentValidation;

namespace Bookstore.Application.Validators.Suppliers
{
    public class UpdateSupplierDtoValodator : AbstractValidator<UpdateSupplierDto>
    {
        public UpdateSupplierDtoValodator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.ContactPerson).MaximumLength(100);
            RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email)).MaximumLength(256);
            RuleFor(x => x.Phone).MaximumLength(50);
            RuleFor(x => x.Address).MaximumLength(500);
        }
    }
}
