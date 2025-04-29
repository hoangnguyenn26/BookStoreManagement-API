using Bookstore.Application.Dtos.Inventory;
using Bookstore.Domain.Enums;
using FluentValidation;

namespace Bookstore.Application.Validators.Inventory
{
    public class AdjustInventoryRequestDtoValidator : AbstractValidator<AdjustInventoryRequestDto>
    {
        public AdjustInventoryRequestDtoValidator()
        {
            RuleFor(x => x.BookId).NotEmpty();
            RuleFor(x => x.ChangeQuantity).NotEqual(0).WithMessage("Change quantity cannot be zero.");

            RuleFor(x => x.Reason)
                .IsInEnum().WithMessage("Invalid reason provided.")
                // Chỉ cho phép các lý do điều chỉnh thủ công
                .Must(reason => reason == InventoryReason.Adjustment)
                .WithMessage("Reason is not valid for manual adjustment.");

            RuleFor(x => x.Notes).MaximumLength(500);
        }
    }
}