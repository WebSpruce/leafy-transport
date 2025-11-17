using FluentValidation;
using leafy_transport.api.Data;
using leafy_transport.api.Endpoints.User;
using Microsoft.EntityFrameworkCore;

namespace leafy_transport.api.Validators.User;

public class UpdateUserRequestValidator : AbstractValidator<UpdateRequest>
{
    private readonly ApplicationDbContext _context;
    public UpdateUserRequestValidator(ApplicationDbContext context)
    {
        _context = context;
        RuleFor(x => x.FirstName)
            .MinimumLength(2).WithMessage("FirstName must contain at least 2 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.FirstName));
        
        RuleFor(x => x.LastName)
            .MinimumLength(2).WithMessage("LastName must contain at least 2 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.LastName));
        
        RuleFor(x => x.UserName)
            .MinimumLength(2).WithMessage("UserName must contain at least 2 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.UserName));
        ;

        RuleFor(x => x.VehicleId)
            .MustAsync(async (vehicleId, cancellationToken) =>
            {
                if (vehicleId.HasValue)
                    return await _context.Vehicles.AnyAsync(v => v.Id == vehicleId.Value, cancellationToken);
                else
                    return true;
            })
            .When(x => x.VehicleId.HasValue)
            .WithMessage("The specified Vehicle does not exist.");

        RuleFor(x => x.PhoneNumber)
            .MinimumLength(9)
            .WithMessage("Phone number must contain at least 9 digits")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }
}