using FluentValidation;
using leafy_transport.api.Data;
using leafy_transport.api.Endpoints.Invoice;
using Microsoft.EntityFrameworkCore;

namespace leafy_transport.api.Validators.Invoice;

public class UpdateRequestValidator : AbstractValidator<UpdateRequest>
{
    private readonly ApplicationDbContext _context;
    public UpdateRequestValidator(ApplicationDbContext context)
    {
        _context = context;
        
        RuleFor(x => x.InvoiceNumber)
            .MinimumLength(2).WithMessage("InvoiceNumber must contain at least 2 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.InvoiceNumber));
        
        RuleFor(x => x.Status)
            .MinimumLength(2).WithMessage("Status must contain at least 2 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Status));

        RuleFor(x => x.ClientId)
            .MustAsync(async (clientId, cancellationToken) =>
            {
                return await _context.Clients.AnyAsync(v => v.Id == clientId, cancellationToken);
            })
            .When(x => x.ClientId != null)
            .WithMessage("The specified Client does not exist.");;
        
        RuleFor(x => x.VehicleId)
            .MustAsync(async (vehicleId, cancellationToken) =>
            {
                return await _context.Vehicles.AnyAsync(v => v.Id == vehicleId, cancellationToken);
            })
            .When(x => x.VehicleId != null)
            .WithMessage("The specified Vehicle does not exist.");;
    }
}