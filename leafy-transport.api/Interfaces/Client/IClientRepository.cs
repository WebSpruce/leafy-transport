using leafy_transport.api.Endpoints.Client;
using leafy_transport.models.Models;

namespace leafy_transport.api.Interfaces.Client;

public interface IClientRepository
{
    Task<Result<models.Models.Client>> CreateAsync(CreateRequest request, CancellationToken token);
    Task<Result<PagedList<models.Models.Client>>>  GetAsync(GetRequest request, CancellationToken token);
    Task<Result> UpdateAsync(Guid id, UpdateRequest request, CancellationToken token);
    Task<Result> DeleteAsync(Guid id, CancellationToken token);
}