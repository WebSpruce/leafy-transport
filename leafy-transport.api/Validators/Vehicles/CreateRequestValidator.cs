using FluentValidation;
using leafy_transport.api.Endpoints.Vehicle;

namespace leafy_transport.api.Validators.Vehicles;

public class CreateRequestValidator : AbstractValidator<CreateRequest>
{
    public CreateRequestValidator()
    {
        RuleFor(x => x.Type)
            .MinimumLength(2).WithMessage("Type must contain at least 2 characters");
        
        RuleFor(x => x.Status)
            .MinimumLength(2).WithMessage("Type must contain at least 2 characters");
        
        RuleFor(x => x.MaxWeight)
            .GreaterThan(0).WithMessage("Max weight should be greater than 0");
    }
}