using leafy_transport.api.Endpoints.Company;
using leafy_transport.models.Models;

namespace leafy_transport.api.Interfaces.Company;

public interface ICompanyRepository
{
    Task<Result<models.Models.Company>> CreateAsync(CreateRequest request, CancellationToken token);
    Task<Result<PagedList<models.Models.Company>>> GetAsync(GetRequest request, CancellationToken token);
    Task<Result> UpdateAsync(Guid id, UpdateRequest request, CancellationToken token);
    Task<Result> DeleteAsync(Guid id, string userId, CancellationToken token);
}