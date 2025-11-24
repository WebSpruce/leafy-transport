using FluentValidation;
using leafy_transport.api.Endpoints.Products;

namespace leafy_transport.api.Validators.Product;

public class UpdateRequestValidator : AbstractValidator<UpdateRequest>
{
    public UpdateRequestValidator()
    {
        RuleFor(x => x.Name)
            .MinimumLength(2).WithMessage("Name must contain at least 2 characters");

        RuleFor(x => x.Weight)
            .GreaterThanOrEqualTo(0).WithMessage("Weight must be greater or equal to 0");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be greater or equal to 0");
    }
}