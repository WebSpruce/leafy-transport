using leafy_transport.api.Endpoints.User;
using leafy_transport.models.Models;

namespace leafy_transport.api.Interfaces.User;

public interface IUserRepository
{
    Task<Result> RegisterUserAsync(RegisterRequest request, CancellationToken token);
    Task<Result> LoginAsync(LoginRequest request, CancellationToken token);
}