using leafy_transport.api.Endpoints.Vehicle;
using leafy_transport.models.Models;

namespace leafy_transport.api.Interfaces.Vehicle;

public interface IVehicleRepository
{
    Task<Result> CreateVehiclesAsync(CreateRequest request, CancellationToken token);
    Task<Result> GetVehiclesAsync(GetRequest request, CancellationToken token);
}