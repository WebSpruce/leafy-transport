using FluentValidation;
using leafy_transport.api.Endpoints.User;

namespace leafy_transport.api.Validators.User;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is empty")
            .EmailAddress().WithMessage("Email address is invalid");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is empty");
    }   
}