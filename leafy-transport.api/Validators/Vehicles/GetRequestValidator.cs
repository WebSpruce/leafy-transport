using FluentValidation;
using leafy_transport.api.Endpoints.Vehicle;

namespace leafy_transport.api.Validators.Vehicles;

public class GetRequestValidator : AbstractValidator<GetRequest>
{
    public GetRequestValidator()
    {
        RuleFor(x => x.Type)
            .MinimumLength(2).WithMessage("Type must contain at least 2 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Type));
        
        RuleFor(x => x.Status)
            .MinimumLength(2).WithMessage("Type must contain at least 2 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Status));
        
        RuleFor(x => x.MaxWeight)
            .GreaterThan(0).WithMessage("Max weight should be greater than 0")
            .When(x => x.MaxWeight != null);
    }
}