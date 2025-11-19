using leafy_transport.api.Endpoints.User;
using leafy_transport.models.Models;

namespace leafy_transport.api.Interfaces.User;

public interface IUserRepository
{
    Task<Result> RegisterUserAsync(RegisterRequest request, CancellationToken token);
    Task<Result<string>> LoginAsync(LoginRequest request, CancellationToken token);
    Task<Result<PagedList<ApplicationUser>>> GetAllAsync(GetRequest request, CancellationToken token);
    Task<Result> UpdateAsync(string id, UpdateRequest request, CancellationToken token);
    Task<Result> DeleteAsync(string id, CancellationToken token);
}