using FluentValidation;
using leafy_transport.api.Data;
using leafy_transport.api.Endpoints.Vehicle;
using Microsoft.EntityFrameworkCore;

namespace leafy_transport.api.Validators.Vehicles;

public class CreateRequestValidator : AbstractValidator<CreateRequest>
{
    private readonly ApplicationDbContext _context;
    public CreateRequestValidator(ApplicationDbContext context)
    {
        _context = context;
        
        RuleFor(x => x.CompanyId)
            .NotNull().WithMessage("CompanyId cannot be null")
            .NotEqual(Guid.Empty).WithMessage("CompanyId cannot be empty")
            .MustAsync(async (companyId, cancellationToken) =>
            {
                return await _context.Companies.AnyAsync(v => v.Id == companyId, cancellationToken);
            })
            .WithMessage("The specified Company does not exist.");
        
        RuleFor(x => x.Type)
            .MinimumLength(2).WithMessage("Type must contain at least 2 characters");
        
        RuleFor(x => x.Status)
            .MinimumLength(2).WithMessage("Type must contain at least 2 characters");
        
        RuleFor(x => x.MaxWeight)
            .GreaterThan(0).WithMessage("Max weight should be greater than 0");
    }
}