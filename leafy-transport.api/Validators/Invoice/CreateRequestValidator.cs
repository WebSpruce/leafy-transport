using FluentValidation;
using leafy_transport.api.Data;
using leafy_transport.api.Endpoints.Invoice;
using Microsoft.EntityFrameworkCore;

namespace leafy_transport.api.Validators.Invoice;

public class CreateRequestValidator : AbstractValidator<CreateRequest>
{
    private readonly ApplicationDbContext _context;
    public CreateRequestValidator(ApplicationDbContext context)
    {
        _context = context;
        
        RuleFor(x => x.InvoiceNumber)
            .MinimumLength(2).WithMessage("InvoiceNumber must contain at least 2 characters");
        
        RuleFor(x => x.Status)
            .MinimumLength(2).WithMessage("Status must contain at least 2 characters");
        
        RuleFor(x => x.TotalWeight)
            .GreaterThan(0).WithMessage("Total Weight should be greater than 0");
        
        RuleFor(x => x.TotalQuantity)
            .GreaterThan(0).WithMessage("Total Quantity should be greater than 0");

        RuleFor(x => x.ClientId)
            .NotNull().WithMessage("ClientId cannot be null")
            .NotEqual(Guid.Empty).WithMessage("ClientId cannot be empty")
            .MustAsync(async (clientId, cancellationToken) =>
            {
                return await _context.Clients.AnyAsync(v => v.Id == clientId, cancellationToken);
            })
            .WithMessage("The specified Client does not exist.");
        
        RuleFor(x => x.VehicleId)
            .NotNull().WithMessage("ClientId cannot be null")
            .NotEqual(Guid.Empty).WithMessage("ClientId cannot be empty")
            .MustAsync(async (vehicleId, cancellationToken) =>
            {
                return await _context.Vehicles.AnyAsync(v => v.Id == vehicleId, cancellationToken);
            })
            .When(x => x.VehicleId.HasValue)
            .WithMessage("The specified Vehicle does not exist.");
    }
}