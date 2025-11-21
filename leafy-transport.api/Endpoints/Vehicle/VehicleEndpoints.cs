using leafy_transport.api.Interfaces;
using leafy_transport.api.Interfaces.Vehicle;
using leafy_transport.models.Models;

namespace leafy_transport.api.Endpoints.Vehicle;

public class VehicleEndpoints : IModule
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        var vehicles = app.MapGroup(ApiRoutes.Vehicles.GroupName)
            .WithTags("Vehicles");
        
        vehicles.MapPost("", async (
            CreateRequest request, 
            IVehicleRepository vehiclesRepository,
            CancellationToken token) =>
        {
            var result = await vehiclesRepository.CreateVehiclesAsync(request, token);

            if (result.IsCancelled)
                return Results.StatusCode(499);

            if (result.IsValidationFailure)
            {
                var problems = new HttpValidationProblemDetails(result.ValidationErrors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed",
                    Detail = "Validation errors occurred",
                    Instance = "/users"
                };
                return Results.Problem(problems);
            }

            if (!result.IsSuccess)
                return Results.BadRequest(result.Errors);

            return Results.Ok(result.Value);
        }).RequireAuthorization(policy => policy.RequireRole(Roles.Admin));

        vehicles.MapGet("", async (
                Guid? id, 
                string? type, 
                double? maxWeight, 
                string? status,
                int? page,
                int? pageSize,
                IVehicleRepository vehiclesRepository,
                CancellationToken token) =>
        {
            var request = new GetRequest(id, type, maxWeight, status, new PaginationRequest(page, pageSize));
            var result = await vehiclesRepository.GetVehiclesAsync(request, token);
            
            if (result.IsCancelled)
                return Results.StatusCode(499);

            if (result.IsValidationFailure)
            {
                var problem = new HttpValidationProblemDetails(result.ValidationErrors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed",
                    Detail = "Validation errors occurred",
                    Instance = $"/vehicles"
                };
                return Results.Problem(problem);
            }
            
            if (result.Errors?.Any() == true)
                return Results.NotFound(result.Errors?.FirstOrDefault());

            return Results.Ok(result.Value);
        }).RequireAuthorization();

        vehicles.MapPatch("/{id}", async (
            Guid id,
            UpdateRequest request,
            IVehicleRepository vehicleRepository,
            CancellationToken token
            ) =>
        {
            var result = await vehicleRepository.UpdateVehicleAsync(id, request, token);

            if (result.IsCancelled)
                return Results.StatusCode(499);

            if (result.IsValidationFailure)
            {
                var problem = new HttpValidationProblemDetails(result.ValidationErrors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed",
                    Detail = "Validation errors occurred",
                    Instance = $"/vehicles/{id}"
                };
                return Results.Problem(problem);
            }

            if (result.Errors?.Any() == true)
                return Results.NotFound(result.Errors?.FirstOrDefault());

            return Results.Ok();
        }).RequireAuthorization(policy => policy.RequireRole(Roles.Admin));;
        
        vehicles.MapDelete("/{id}", async (
            Guid id,
            IVehicleRepository vehicleRepository,
            CancellationToken token
        ) =>
        {
            var result = await vehicleRepository.DeleteVehicleAsync(id, token);

            if (result.IsCancelled)
                return Results.StatusCode(499);

            if (result.Errors?.Any() == true)
                return Results.NotFound(result.Errors?.FirstOrDefault());

            return Results.Ok();
        }).RequireAuthorization(policy => policy.RequireRole(Roles.Admin));;
    }
}