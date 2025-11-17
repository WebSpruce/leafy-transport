using FluentValidation;
using leafy_transport.api.Endpoints.User;

namespace leafy_transport.api.Validators.User;

internal class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is empty")
            .EmailAddress().WithMessage("Email address is invalid");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is empty")
            .MinimumLength(6).WithMessage("Password must contain at least 6 characters")
            .Must(c => c.Any(char.IsUpper)).WithMessage("Password must contain an uppercase letter")
            .Must(c => c.Any(char.IsLower)).WithMessage("Password must contain an lowercase letter")
            .Must(c => c.Any(char.IsDigit)).WithMessage("Password must contain a digit");
        
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("FirstName is empty")
            .MinimumLength(2).WithMessage("FirstName must contain at least 2 characters");
        
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("LastName is empty")
            .MinimumLength(2).WithMessage("LastName must contain at least 2 characters");
        
        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is empty");
    }
}