using FluentValidation;
using leafy_transport.api.Data;
using leafy_transport.api.Endpoints.Vehicle;
using leafy_transport.api.Infrastructure;
using leafy_transport.api.Interfaces.Vehicle;
using leafy_transport.models.Models;
using Microsoft.EntityFrameworkCore;

namespace leafy_transport.api.Repositories.Vehicle;

public class VehicleRepository : IVehicleRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IValidator<GetRequest> _validatorGet;
    private readonly IValidator<CreateRequest> _validatorCreate;
    private readonly IValidator<UpdateRequest> _validatorUpdate;
    public VehicleRepository(ApplicationDbContext dbContext, IValidator<GetRequest> validatorGet, IValidator<CreateRequest> validatorCreate, IValidator<UpdateRequest> validatorUpdate)
    {
        _dbContext = dbContext;
        _validatorGet = validatorGet;
        _validatorCreate = validatorCreate;
        _validatorUpdate = validatorUpdate;
    }

    public async Task<Result<models.Models.Vehicle>> CreateVehiclesAsync(CreateRequest request, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Result.Cancelled<models.Models.Vehicle>();
            
        var validationResult = _validatorCreate.Validate(request);
        if (!validationResult.IsValid)
            return Result.ValidationFailure<models.Models.Vehicle>(new Dictionary<string, string[]>(validationResult.ToDictionary()));

        var vehicle = new models.Models.Vehicle()
        {
            Id = Guid.NewGuid(),
            Status = request.Status,
            Type = request.Type,
            MaxWeight = request.MaxWeight
        };
        
        await _dbContext.Vehicles.AddAsync(vehicle, token);
        await _dbContext.SaveChangesAsync(token);
        
        return Result.Success(vehicle);
    }

    public async Task<Result<PagedList<models.Models.Vehicle>>> GetVehiclesAsync(GetRequest request, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Result.Cancelled<PagedList<models.Models.Vehicle>>();

        var validationResult = _validatorGet.Validate(request);
        if (!validationResult.IsValid)
            return Result.ValidationFailure<PagedList<models.Models.Vehicle>>(new Dictionary<string, string[]>(validationResult.ToDictionary()));

        var vehicles = _dbContext.Vehicles
            .AsNoTracking()
            .Where(vehicle => 
                (request.Id == null || vehicle.Id == request.Id) &&
                (string.IsNullOrEmpty(request.Type) || vehicle.Type.ToLower() == request.Type.ToLower()) &&
                (string.IsNullOrEmpty(request.Status) || vehicle.Status.ToLower() == request.Status.ToLower()) &&
                (request.MaxWeight == null || vehicle.MaxWeight == request.MaxWeight.Value)
            ).AsQueryable();

        var result = await Pagination.Paginate(vehicles, request.pagination?.pageNumber, request.pagination?.pageSize, token);

        return Result.Success(result);
    }

    public async Task<Result> UpdateVehicleAsync(Guid id, UpdateRequest request, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Result.Cancelled();

        var validationResult = _validatorUpdate.Validate(request);
        if (!validationResult.IsValid)
            return Result.ValidationFailure(new Dictionary<string, string[]>(validationResult.ToDictionary()));

        var vehicle = await _dbContext.Vehicles.FirstOrDefaultAsync(x => x.Id == id);
        if (vehicle is null)
            return Result.Failure(new List<object>() { "There is no vehicle with the provided Id" });

        if (request.Type is not null)
            vehicle.Type = request.Type;
        if (request.Status is not null)
            vehicle.Status = request.Status;
        if (request.MaxWeight is not null)
            vehicle.MaxWeight = (double)request.MaxWeight;

        await _dbContext.SaveChangesAsync(token);

        return Result.Success();
    }

    public async Task<Result> DeleteVehicleAsync(Guid id, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Result.Cancelled();
        
        var vehicle = await _dbContext.Vehicles.FirstOrDefaultAsync(x => x.Id == id);
        if (vehicle is null)
            return Result.Failure(new List<object>() { "There is no vehicle with the provided Id" });

        _dbContext.Vehicles.Remove(vehicle);

        await _dbContext.SaveChangesAsync(token);

        return Result.Success();
    }
}