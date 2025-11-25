using FluentValidation;
using leafy_transport.api.Data;
using leafy_transport.api.Endpoints.Invoice;
using leafy_transport.api.Infrastructure;
using leafy_transport.api.Interfaces.Invoice;
using leafy_transport.models.Models;
using Microsoft.EntityFrameworkCore;

namespace leafy_transport.api.Repositories.Invoice;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IValidator<CreateRequest> _validatorCreate;
    private readonly IValidator<GetRequest> _validatorGet;
    private readonly IValidator<UpdateRequest> _validatorUpdate;
    public InvoiceRepository(ApplicationDbContext dbContext, IValidator<CreateRequest> validatorCreate, IValidator<GetRequest> validatorGet, IValidator<UpdateRequest> validatorUpdate)
    {
        _dbContext = dbContext;
        _validatorCreate = validatorCreate;
        _validatorGet = validatorGet;
        _validatorUpdate = validatorUpdate;
    }

    public async Task<Result<models.Models.Invoice>> CreateAsync(CreateRequest request, CancellationToken token)
    {
        if(token.IsCancellationRequested)
            return Result.Cancelled<models.Models.Invoice>();    
        
        var validationResult = await _validatorCreate.ValidateAsync(request, token);
        if(!validationResult.IsValid)
            return Result.ValidationFailure<models.Models.Invoice>(new Dictionary<string, string[]>(validationResult.ToDictionary()));
        
        var invoice = new models.Models.Invoice()
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = request.InvoiceNumber, 
            ClientId = request.ClientId, 
            VehicleId = request.VehicleId ,
            Status = request.Status,
            ParentInvoiceId = request.ParentInvoiceId
        };
        
        await _dbContext.Invoices.AddAsync(invoice, token);
        await _dbContext.SaveChangesAsync(token);
        
        return Result.Success(invoice);
    }

    public async Task<Result<PagedList<models.Models.Invoice>>> GetAsync(GetRequest request, CancellationToken token)
    {
        if(token.IsCancellationRequested)
            return Result.Cancelled<PagedList<models.Models.Invoice>>();    
        
        var validationResult = _validatorGet.Validate(request);
        if(!validationResult.IsValid)
            return Result.ValidationFailure<PagedList<models.Models.Invoice>>(new Dictionary<string, string[]>(validationResult.ToDictionary()));
        
        var invoices = _dbContext.Invoices
            .AsNoTracking()
            .Where(invoice => 
                (request.Id == null || invoice.Id == request.Id) &&
                (string.IsNullOrEmpty(request.InvoiceNumber) || invoice.InvoiceNumber.ToLower() == request.InvoiceNumber.ToLower()) &&
                (request.ClientId == null || invoice.ClientId == request.ClientId) &&
                (request.VehicleId == null || invoice.VehicleId == request.VehicleId) &&
                (string.IsNullOrEmpty(request.Status) || invoice.Status.ToLower() == request.Status.ToLower()) &&
                (request.ParentInvoiceId == null || invoice.ParentInvoiceId == request.ParentInvoiceId)
            ).AsQueryable();

        var result = await Pagination.Paginate(invoices, request.pagination?.pageNumber, request.pagination?.pageSize, token);

        return Result.Success(result);
    }

    public async Task<Result> UpdateAsync(Guid id, UpdateRequest request, CancellationToken token)
    {
        if(token.IsCancellationRequested)
            return Result.Cancelled();    
        
        var validationResult = await _validatorUpdate.ValidateAsync(request, token);
        if(!validationResult.IsValid)
            return Result.ValidationFailure(new Dictionary<string, string[]>(validationResult.ToDictionary()));
        
        var invoice = await _dbContext.Invoices.FirstOrDefaultAsync(x => x.Id == id, token);
        if (invoice is null)
            return Result.Failure(new List<object>() { "There is no invoice with the provided Id" });

        if (request.InvoiceNumber is not null)
            invoice.InvoiceNumber = request.InvoiceNumber;
        if (request.ClientId is not null)
            invoice.ClientId = (Guid)request.ClientId;
        if (request.VehicleId is not null)
            invoice.VehicleId = (Guid)request.VehicleId;
        if (request.Status is not null)
            invoice.Status = request.Status;
        if (request.ParentInvoiceId is not null)
            invoice.ParentInvoiceId = request.ParentInvoiceId;
        
        await _dbContext.SaveChangesAsync(token);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken token)
    {
        if(token.IsCancellationRequested)
            return Result.Cancelled();    
        
        var invoice = await _dbContext.Invoices.FirstOrDefaultAsync(x => x.Id == id, token);
        if (invoice is null)
            return Result.Failure(new List<object>() { "There is no invoice with the provided Id" });

        _dbContext.Invoices.Remove(invoice);

        await _dbContext.SaveChangesAsync(token);

        return Result.Success();
    }
}