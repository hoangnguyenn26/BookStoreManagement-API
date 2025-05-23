using Bookstore.Application.Dtos;
using FluentValidation;

public class LoginViewModelValidator : AbstractValidator<LoginRequestDto>
{
    public LoginViewModelValidator()
    {
        RuleFor(x => x.LoginIdentifier)
            .NotEmpty().WithMessage("Username or Email is required.")
            .MinimumLength(3).WithMessage("Username or Email must be at least 3 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
