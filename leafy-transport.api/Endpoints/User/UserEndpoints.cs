using leafy_transport.api.Interfaces;
using leafy_transport.api.Interfaces.User;
using leafy_transport.models.Models;
using Microsoft.AspNetCore.Mvc;

namespace leafy_transport.api.Endpoints.User;

public class UserEndpoints : IModule
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        var users = app
            .MapGroup(ApiRoutes.Users.GroupName)
            .WithTags("Users");

        users.MapPost("", async (
            RegisterRequest request, 
            IUserRepository userRepository,
            CancellationToken token) =>
        {
            var result = await userRepository.RegisterUserAsync(request, token);

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

            return Results.Ok();
        }).RequireAuthorization(policy => policy.RequireRole(Roles.Admin));
        
        users.MapPost("login", async (
            LoginRequest request,
            IUserRepository userRepository,
        CancellationToken token) =>
        {
            Result result = await userRepository.LoginAsync(request, token);
            if (result.IsCancelled)
                return Results.StatusCode(499);
            
            if (result.IsValidationFailure)
            {
                var problems = new HttpValidationProblemDetails(result.ValidationErrors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed",
                    Detail = "Validation errors occurred",
                    Instance = "/users/login"
                };
                return Results.Problem(problems);
            }

            if(!result.IsSuccess && result.Errors.Any() && result.Errors.Contains("Unauthorized"))
                return Results.Unauthorized();
            
            string? accessToken = null;
            if (result.IsSuccess && result.Values.Any())
                accessToken = result.Values?.FirstOrDefault()?.ToString();

            return Results.Ok(new { accessToken });
        });

        users.MapGet("token-check", (CancellationToken token) =>
        {
            if (token.IsCancellationRequested)
                return Results.StatusCode(499);
            
            return Results.Ok();
        }).RequireAuthorization();
        
        users.MapGet("", async (
            string? id,
            string? email,
            string? firstName,
            string? lastName,
            string? userName,
            string? phoneNumber,
            Guid? vehicleId,
            string? roleName,
            DateTime? createdAt,
            [FromServices] IUserRepository userRepository,
            CancellationToken token
        ) =>
        {
            var request = new GetRequest(
                Id: id,
                Email: email,
                FirstName: firstName,
                LastName: lastName,
                UserName: userName,
                PhoneNumber: phoneNumber,
                VehicleId: vehicleId,
                RoleName: roleName,
                CreatedAt: createdAt
            );
            var result = await userRepository.GetAllAsync(request, token);

            if (result.IsCancelled)
                return Results.StatusCode(499);

            if (result.IsValidationFailure)
            {
                var problem = new HttpValidationProblemDetails(result.ValidationErrors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed",
                    Detail = "Validation errors occurred",
                    Instance = $"/users"
                };
                return Results.Problem(problem);
            }
            
            if (result.Errors?.Any() == true)
                return Results.NotFound(result.Errors?.FirstOrDefault());

            return Results.Ok(result.Values);
        });

        users.MapPatch("/{id}", async (
            string id,
            UpdateRequest request,
            IUserRepository userRepository,
            CancellationToken token
            ) =>
        {
            var result = await userRepository.UpdateAsync(id, request, token);

            if (result.IsCancelled)
                return Results.StatusCode(499);

            if (result.IsValidationFailure)
            {
                var problem = new HttpValidationProblemDetails(result.ValidationErrors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed",
                    Detail = "Validation errors occurred",
                    Instance = $"/users/{id}"
                };
                return Results.Problem(problem);
            }
            
            if (result.Errors?.Any() == true && result.Errors.Contains("There is no user with provided Id"))
                return Results.NotFound("There is no user with provided Id");

            return Results.Ok();
        });

        users.MapDelete("/{id}", async (
            string id,
            IUserRepository userRepository,
            CancellationToken token
        ) =>
        {
            var result = await userRepository.DeleteAsync(id, token);

            if (result.IsCancelled)
                return Results.StatusCode(499);

            if (result.Errors?.Any() == true && result.Errors.Contains("There is no user with provided Id"))
                return Results.NotFound("There is no user with provided Id");
            
            return Results.Ok();
        });
    }
}