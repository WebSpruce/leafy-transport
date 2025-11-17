using leafy_transport.api.Interfaces;
using leafy_transport.api.Interfaces.User;
using leafy_transport.models.Models;

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
    }
}