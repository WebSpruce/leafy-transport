using FluentValidation;
using leafy_transport.api.Endpoints.Invoice;

namespace leafy_transport.api.Validators.Invoice;

public class GetRequestValidator : AbstractValidator<GetRequest>
{
    public GetRequestValidator()
    {
        RuleFor(x => x.InvoiceNumber)
            .MinimumLength(2).WithMessage("InvoiceNumber must contain at least 2 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.InvoiceNumber));
        
        RuleFor(x => x.Status)
            .MinimumLength(2).WithMessage("Status must contain at least 2 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Status));
    }
}