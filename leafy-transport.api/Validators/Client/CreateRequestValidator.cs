using FluentValidation;
using leafy_transport.api.Endpoints.Client;

namespace leafy_transport.api.Validators.Client;

public class CreateRequestValidator : AbstractValidator<CreateRequest>
{
    public CreateRequestValidator()
    {
        RuleFor(x => x.City)
            .NotNull()
            .NotEmpty()
            .WithMessage("City cannot be empty");
        
        RuleFor(x => x.Address)
            .NotNull()
            .NotEmpty()
            .MinimumLength(2)
            .WithMessage("Address must contain at least 2 characters");
        
        RuleFor(x => x.PostCode)
            .NotNull()
            .NotEmpty()
            .MinimumLength(2)
            .WithMessage("Postcode must contain at least 2 characters");
        
        RuleFor(x => x.Location)
            .NotNull()
            .NotEmpty()
            .WithMessage("Location cannot be empty");
    }
}