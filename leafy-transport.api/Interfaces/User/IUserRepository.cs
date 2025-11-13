using leafy_transport.api.Endpoints.User;
using leafy_transport.models.Models;

namespace leafy_transport.api.Interfaces.User;

public interface IUserRepository
{
    Task<Result> RegisterUserAsync(RegisterRequest request, CancellationToken token);
}