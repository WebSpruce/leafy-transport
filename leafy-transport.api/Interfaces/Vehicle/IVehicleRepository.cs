using leafy_transport.api.Endpoints.Vehicle;
using leafy_transport.models.Models;

namespace leafy_transport.api.Interfaces.Vehicle;

public interface IVehicleRepository
{
    Task<Result<models.Models.Vehicle>> CreateVehiclesAsync(CreateRequest request, CancellationToken token);
    Task<Result<PagedList<models.Models.Vehicle>>>  GetVehiclesAsync(GetRequest request, CancellationToken token);
}