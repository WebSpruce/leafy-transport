using FluentValidation;
using leafy_transport.api.Endpoints.User;

namespace leafy_transport.api.Validators.User;

public class GetRequestValidator : AbstractValidator<GetRequest>
{
    public GetRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .MinimumLength(2).WithMessage("FirstName must contain at least 2 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.FirstName));
        
        RuleFor(x => x.LastName)
            .MinimumLength(2).WithMessage("LastName must contain at least 2 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.LastName));
        
        RuleFor(x => x.UserName)
            .MinimumLength(2).WithMessage("UserName must contain at least 2 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.UserName));
        
        RuleFor(x => x.RoleName)
            .MinimumLength(2).WithMessage("RoleName must contain at least 2 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.RoleName));

        RuleFor(x => x.PhoneNumber)
            .MinimumLength(9)
            .WithMessage("Phone number must contain at least 9 digits")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }
}