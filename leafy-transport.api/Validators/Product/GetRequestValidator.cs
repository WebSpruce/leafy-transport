using FluentValidation;
using leafy_transport.api.Endpoints.Products;

namespace leafy_transport.api.Validators.Product;

public class GetRequestValidator : AbstractValidator<GetRequest>
{
    public GetRequestValidator()
    {
        RuleFor(x => x.Name)
            .MinimumLength(2).WithMessage("Name must contain at least 2 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));
    }
}