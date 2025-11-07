using leafy_transport.api.Data;
using leafy_transport.api.Interfaces;
using leafy_transport.models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace leafy_transport.api.Endpoints;

public class UserEndpoints : IModule
{
    public record Request(string Email, string Password, string FirstName, string LastName, string Role);

    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        var users = app
            .MapGroup(ApiRoutes.Users.GroupName)
            .WithTags("Users");

        users.MapPost("register", async (
            Request request, 
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager) =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                CreatedAt = DateTime.UtcNow,
            };
            
            IdentityResult identityResult = await userManager.CreateAsync(user, request.Password);
            if (!identityResult.Succeeded)
            {
                return Results.BadRequest(identityResult.Errors);
            }

            IdentityResult roleResult = await userManager.AddToRoleAsync(user, request.Role);
            if (!roleResult.Succeeded)
            {
                return Results.BadRequest(roleResult.Errors);
            }

            await transaction.CommitAsync();

            return Results.Ok();
        }).RequireAuthorization("RequireAdminRole");
    }
}