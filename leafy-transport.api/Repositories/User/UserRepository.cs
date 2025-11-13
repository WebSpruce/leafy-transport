using FluentValidation;
using leafy_transport.api.Data;
using leafy_transport.api.Endpoints.User;
using leafy_transport.api.Interfaces.User;
using leafy_transport.models.Models;
using Microsoft.AspNetCore.Identity;

namespace leafy_transport.api.Repositories.User;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<RegisterRequest> _validator;

    public UserRepository(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IValidator<RegisterRequest> validator)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _validator = validator;
    }
    public async Task<Result> RegisterUserAsync(RegisterRequest request, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Result.Cancelled();
            
        var validationResult = _validator.Validate(request);
        if (!validationResult.IsValid)
            return Result.ValidationFailure(new Dictionary<string, string[]>(validationResult.ToDictionary()));
            
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(token);
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = DateTime.UtcNow,
        };
            
        IdentityResult identityResult = await _userManager.CreateAsync(user, request.Password);
        if (!identityResult.Succeeded)
        {
            return Result.Failure(identityResult.Errors);
        }

        IdentityResult roleResult = await _userManager.AddToRoleAsync(user, request.Role);
        if (!roleResult.Succeeded)
        {
            return Result.Failure(roleResult.Errors);
        }

        await transaction.CommitAsync(token);
        
        return Result.Success();
    }
}