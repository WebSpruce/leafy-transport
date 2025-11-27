using leafy_transport.api.Endpoints.Invoice;
using leafy_transport.models.Models;

namespace leafy_transport.api.Interfaces.Invoice;

public interface IInvoiceRepository
{
    Task<Result<models.Models.Invoice>> CreateAsync(CreateRequest request, CancellationToken token);
    Task<Result<PagedList<models.Models.Invoice>>> GetAsync(GetRequest request, CancellationToken token);
    Task<Result> UpdateAsync(Guid id, UpdateRequest request, CancellationToken token);
    Task<Result> DeleteAsync(Guid id, Guid companyId, CancellationToken token);
}