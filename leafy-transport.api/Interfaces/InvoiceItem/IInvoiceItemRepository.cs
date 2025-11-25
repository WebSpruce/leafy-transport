using leafy_transport.api.Endpoints.InvoiceItem;
using leafy_transport.models.Models;

namespace leafy_transport.api.Interfaces.InvoiceItem;

public interface IInvoiceItemRepository
{
    Task<Result<models.Models.InvoiceItem>> CreateAsync(CreateRequest request, CancellationToken token);
    Task<Result<PagedList<models.Models.InvoiceItem>>> GetAsync(GetRequest request, CancellationToken token);
    Task<Result> UpdateAsync(Guid id, UpdateRequest request, CancellationToken token);
    Task<Result> DeleteAsync(Guid id, CancellationToken token);
}