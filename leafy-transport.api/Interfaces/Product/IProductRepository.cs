using leafy_transport.api.Endpoints.Products;
using leafy_transport.models.Models;

namespace leafy_transport.api.Interfaces.Product;

public interface IProductRepository
{
    Task<Result<models.Models.Product>> CreateAsync(CreateRequest request, CancellationToken token);
    Task<Result<PagedList<models.Models.Product>>>  GetAsync(GetRequest request, CancellationToken token);
    Task<Result> UpdateAsync(Guid id, UpdateRequest request, CancellationToken token);
    Task<Result> DeleteAsync(Guid id, CancellationToken token);
}