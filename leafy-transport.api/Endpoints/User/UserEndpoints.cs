using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using FluentValidation;
using leafy_transport.api.Data;
using leafy_transport.api.Interfaces;
using leafy_transport.api.Validators.User;
using leafy_transport.models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace leafy_transport.api.Endpoints.User;

public class UserEndpoints : IModule
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        var users = app
            .MapGroup(ApiRoutes.Users.GroupName)
            .WithTags("Users");

        users.MapPost("users", async (
            RegisterRequest request, 
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            IValidator<RegisterRequest> validator,
            CancellationToken token) =>
        {
            if (token.IsCancellationRequested)
                return Results.StatusCode(499);
            
            var validationResult = validator.Validate(request);
            if (!validationResult.IsValid)
            {
                var problems = new HttpValidationProblemDetails(validationResult.ToDictionary())
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed",
                    Detail = "Validation errors occurred",
                    Instance = "/users"
                };
                return Results.Problem(problems);
            }
            
            await using var transaction = await dbContext.Database.BeginTransactionAsync(token);
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

            await transaction.CommitAsync(token);

            return Results.Ok();
        }).RequireAuthorization(policy => policy.RequireRole(Roles.Admin));
        
        users.MapPost("login", async (
            LoginRequest request,
            UserManager<ApplicationUser> userManager,
            IOptions<JwtSettings> jwtSettings,
            IValidator<LoginRequest> validator,
            CancellationToken token) =>
        {
            if (token.IsCancellationRequested)
                return Results.StatusCode(499);

            var validationResult = validator.Validate(request);
            if (!validationResult.IsValid)
            {
                var problems = new HttpValidationProblemDetails(validationResult.ToDictionary())
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed",
                    Detail = "Validation errors occurred",
                    Instance = "/users/login"
                };
                return Results.Problem(problems);
            }
            
            var config = jwtSettings.Value;
            var user = await userManager.FindByEmailAsync(request.Email);

            if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            {
                return Results.Unauthorized();
            }

            var roles = await userManager.GetRolesAsync(user);

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.SecretKey));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            List<Claim> claims =
            [
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email!),
                ..roles.Select(r => new Claim(ClaimTypes.Role, r))
            ];

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(config.ExpirationInMinutes),
                SigningCredentials = credentials,
                Issuer = config.Issuer,
                Audience = config.Audience
            };

            var tokenHandler = new JsonWebTokenHandler();
            string accessToken = tokenHandler.CreateToken(tokenDescriptor);
            
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