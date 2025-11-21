using FluentValidation;
using leafy_transport.api.Data;
using leafy_transport.api.Endpoints.Products;
using leafy_transport.api.Infrastructure;
using leafy_transport.api.Interfaces.Product;
using leafy_transport.models.Models;
using Microsoft.EntityFrameworkCore;

namespace leafy_transport.api.Repositories.Product;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IValidator<GetRequest> _validatorGet;
    private readonly IValidator<CreateRequest> _validatorCreate;
    private readonly IValidator<UpdateRequest> _validatorUpdate;
    public ProductRepository(ApplicationDbContext dbContext, IValidator<GetRequest> validatorGet, IValidator<CreateRequest> validatorCreate, IValidator<UpdateRequest> validatorUpdate)
    {
        _dbContext = dbContext;
        _validatorGet = validatorGet;
        _validatorCreate = validatorCreate;
        _validatorUpdate = validatorUpdate;
    }
    public async Task<Result<models.Models.Product>> CreateAsync(CreateRequest request, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Result.Cancelled<models.Models.Product>();
            
        var validationResult = _validatorCreate.Validate(request);
        if (!validationResult.IsValid)
            return Result.ValidationFailure<models.Models.Product>(new Dictionary<string, string[]>(validationResult.ToDictionary()));

        var product = new models.Models.Product()
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            TotalWeight = request.Weight
        };
        
        await _dbContext.Products.AddAsync(product, token);
        await _dbContext.SaveChangesAsync(token);
        
        return Result.Success(product);
    }

    public async Task<Result<PagedList<models.Models.Product>>> GetAsync(GetRequest request, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Result.Cancelled<PagedList<models.Models.Product>>();

        var validationResult = _validatorGet.Validate(request);
        if (!validationResult.IsValid)
            return Result.ValidationFailure<PagedList<models.Models.Product>>(new Dictionary<string, string[]>(validationResult.ToDictionary()));

        var products = _dbContext.Products
            .AsNoTracking()
            .Where(product => 
                (request.Id == null || product.Id == request.Id) &&
                (string.IsNullOrEmpty(request.Name) || product.Name.ToLower() == request.Name.ToLower()) &&
                (request.Weight == null || product.TotalWeight == request.Weight.Value)
            ).AsQueryable();

        var result = await Pagination.Paginate(products, request.pagination?.pageNumber, request.pagination?.pageSize, token);

        return Result.Success(result);
    }

    public async Task<Result> UpdateAsync(Guid id, UpdateRequest request, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Result.Cancelled();

        var validationResult = _validatorUpdate.Validate(request);
        if (!validationResult.IsValid)
            return Result.ValidationFailure(new Dictionary<string, string[]>(validationResult.ToDictionary()));

        var product = await _dbContext.Products.FirstOrDefaultAsync(x => x.Id == id, token);
        if (product is null)
            return Result.Failure(new List<object>() { "There is no product with the provided Id" });

        if (request.Name is not null)
            product.Name = request.Name;
        if (request.Weight is not null)
            product.TotalWeight = (int)request.Weight;


        await _dbContext.SaveChangesAsync(token);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Result.Cancelled();
        
        var product = await _dbContext.Products.FirstOrDefaultAsync(x => x.Id == id, token);
        if (product is null)
            return Result.Failure(new List<object>() { "There is no product with the provided Id" });

        _dbContext.Products.Remove(product);

        await _dbContext.SaveChangesAsync(token);

        return Result.Success();
    }
}