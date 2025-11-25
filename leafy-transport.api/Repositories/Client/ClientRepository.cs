using FluentValidation;
using leafy_transport.api.Data;
using leafy_transport.api.Endpoints.Client;
using leafy_transport.api.Infrastructure;
using leafy_transport.api.Interfaces.Client;
using leafy_transport.models.Models;
using Microsoft.EntityFrameworkCore;

namespace leafy_transport.api.Repositories.Client;

public class ClientRepository : IClientRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IValidator<GetRequest> _validatorGet;
    private readonly IValidator<CreateRequest> _validatorCreate;
    private readonly IValidator<UpdateRequest> _validatorUpdate;
    public ClientRepository(ApplicationDbContext dbContext, IValidator<GetRequest> validatorGet, IValidator<CreateRequest> validatorCreate, IValidator<UpdateRequest> validatorUpdate)
    {
        _dbContext = dbContext;
        _validatorGet = validatorGet;
        _validatorCreate = validatorCreate;
        _validatorUpdate = validatorUpdate;
    }
    public async Task<Result<models.Models.Client>> CreateAsync(CreateRequest request, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Result.Cancelled<models.Models.Client>();
            
        var validationResult = _validatorCreate.Validate(request);
        if (!validationResult.IsValid)
            return Result.ValidationFailure<models.Models.Client>(new Dictionary<string, string[]>(validationResult.ToDictionary()));

        var client = new models.Models.Client()
        {
            Id = Guid.NewGuid(),
            City = request.City,
            Address = request.Address,
            Postcode = request.PostCode,
            Location = request.Location
        };
        
        await _dbContext.Clients.AddAsync(client, token);
        await _dbContext.SaveChangesAsync(token);
        
        return Result.Success(client);
    }

    public async Task<Result<PagedList<models.Models.Client>>> GetAsync(GetRequest request, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Result.Cancelled<PagedList<models.Models.Client>>();

        var validationResult =await  _validatorGet.ValidateAsync(request, token);
        if (!validationResult.IsValid)
            return Result.ValidationFailure<PagedList<models.Models.Client>>(new Dictionary<string, string[]>(validationResult.ToDictionary()));

        IQueryable<models.Models.Client> clients = _dbContext.Clients.AsNoTracking();
        
        if (!string.IsNullOrEmpty(request.UserId))
            clients = clients.Where(client => client.Users.Any(u => u.Id == request.UserId));
        
        
        clients = clients
            .Where(client => 
                (request.Id == null || client.Id == request.Id) &&
                (string.IsNullOrEmpty(request.City) || client.City.ToLower() == request.City.ToLower()) &&
                (string.IsNullOrEmpty(request.Address) || client.Address.ToLower() == request.Address.ToLower()) &&
                (string.IsNullOrEmpty(request.PostCode) || client.Postcode.ToLower() == request.PostCode.ToLower()) &&
                (string.IsNullOrEmpty(request.Location) || client.Location.ToLower() == request.Location.ToLower())
            ).AsQueryable();

        var result = await Pagination.Paginate(clients, request.pagination?.pageNumber, request.pagination?.pageSize, token);

        return Result.Success(result);
    }

    public async Task<Result> UpdateAsync(Guid id, UpdateRequest request, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Result.Cancelled();

        var validationResult = _validatorUpdate.Validate(request);
        if (!validationResult.IsValid)
            return Result.ValidationFailure(new Dictionary<string, string[]>(validationResult.ToDictionary()));

        var client = await _dbContext.Clients.FirstOrDefaultAsync(x => x.Id == id, token);
        if (client is null)
            return Result.Failure(new List<object>() { "There is no client with the provided Id" });

        if (request.City is not null)
            client.City = request.City;
        if (request.Address is not null)
            client.Address = request.Address;
        if (request.PostCode is not null)
            client.Postcode = request.PostCode;
        if (request.Location is not null)
            client.Location = request.Location;

        await _dbContext.SaveChangesAsync(token);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Result.Cancelled();
        
        var client = await _dbContext.Clients.FirstOrDefaultAsync(x => x.Id == id, token);
        if (client is null)
            return Result.Failure(new List<object>() { "There is no client with the provided Id" });

        _dbContext.Clients.Remove(client);

        await _dbContext.SaveChangesAsync(token);

        return Result.Success();
    }
}