using FluentValidation;
using leafy_transport.api.Endpoints.Company;

namespace leafy_transport.api.Validators.Company;

public class GetRequestValidator : AbstractValidator<GetRequest>
{
    public GetRequestValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Slug)
            .MaximumLength(50).When(x => !string.IsNullOrEmpty(x.Slug));
    }
}