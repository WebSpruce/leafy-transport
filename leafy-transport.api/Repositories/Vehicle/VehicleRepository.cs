using FluentValidation;
using leafy_transport.api.Data;
using leafy_transport.api.Endpoints.Vehicle;
using leafy_transport.api.Interfaces.Vehicle;
using leafy_transport.models.Models;

namespace leafy_transport.api.Repositories.Vehicle;

public class VehicleRepository : IVehicleRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IValidator<GetRequest> _validatorGet;
    private readonly IValidator<CreateRequest> _validatorCreate;
    public VehicleRepository(ApplicationDbContext dbContext, IValidator<GetRequest> validatorGet, IValidator<CreateRequest> validatorCreate)
    {
        _dbContext = dbContext;
        _validatorGet = validatorGet;
        _validatorCreate = validatorCreate;
    }

    public async Task<Result> CreateVehiclesAsync(CreateRequest request, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Result.Cancelled();
            
        var validationResult = _validatorCreate.Validate(request);
        if (!validationResult.IsValid)
            return Result.ValidationFailure(new Dictionary<string, string[]>(validationResult.ToDictionary()));

        var vehicle = new models.Models.Vehicle()
        {
            Id = Guid.NewGuid(),
            Status = request.Status,
            Type = request.Type,
            MaxWeight = request.MaxWeight
        };
        
        await _dbContext.Vehicles.AddAsync(vehicle, token);
        await _dbContext.SaveChangesAsync(token);
        
        return Result.Success(new List<object>(){vehicle});
    }

    public async Task<Result> GetVehiclesAsync(GetRequest request, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Result.Cancelled();

        var validationResult = _validatorGet.Validate(request);
        if (!validationResult.IsValid)
            return Result.ValidationFailure(new Dictionary<string, string[]>(validationResult.ToDictionary()));

        var vehicles = _dbContext.Vehicles
            .Where(vehicle => 
                (request.Id == null || vehicle.Id == request.Id) &&
                (string.IsNullOrEmpty(request.Type) || vehicle.Type.ToLower() == request.Type.ToLower()) &&
                (string.IsNullOrEmpty(request.Status) || vehicle.Status.ToLower() == request.Status.ToLower()) &&
                (request.MaxWeight == null || vehicle.MaxWeight == request.MaxWeight)
            ).AsQueryable();

        return Result.Success(vehicles);
    }
}