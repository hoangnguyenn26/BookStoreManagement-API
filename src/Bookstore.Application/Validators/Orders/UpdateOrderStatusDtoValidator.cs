
using Bookstore.Application.Dtos.Orders;
using FluentValidation;

namespace Bookstore.Application.Validators.Orders
{
    public class UpdateOrderStatusDtoValidator : AbstractValidator<UpdateOrderStatusDto>
    {
        public UpdateOrderStatusDtoValidator()
        {
            RuleFor(x => x.NewStatus)
                .IsInEnum().WithMessage("Invalid order status provided.");
        }
    }
}