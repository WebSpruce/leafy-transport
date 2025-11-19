using System.Security.Claims;
using System.Text;
using FluentValidation;
using leafy_transport.api.Data;
using leafy_transport.api.Endpoints.User;
using leafy_transport.api.Infrastructure;
using leafy_transport.api.Interfaces.User;
using leafy_transport.models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace leafy_transport.api.Repositories.User;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<RegisterRequest> _validator;
    private readonly IValidator<LoginRequest> _validatorLogin;
    private readonly IValidator<UpdateRequest> _validatorUpdate;
    private readonly IValidator<GetRequest> _validatorGet;
    private readonly IOptions<JwtSettings> _jwtSettings;

    public UserRepository(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, 
        IValidator<RegisterRequest> validator, IValidator<LoginRequest> validatorLogin, IValidator<UpdateRequest> validatorUpdate, 
        IValidator<GetRequest> validatorGet, IOptions<JwtSettings> jwtSettings)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _validator = validator;
        _validatorLogin = validatorLogin;
        _validatorUpdate = validatorUpdate;
        _validatorGet = validatorGet;
        _jwtSettings = jwtSettings;
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

    public async Task<Result<string>> LoginAsync(LoginRequest request, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Result.Cancelled<string>();

        var validationResult = _validatorLogin.Validate(request);
        if (!validationResult.IsValid)
            return Result.ValidationFailure<string>(new Dictionary<string, string[]>(validationResult.ToDictionary()));
        
        var config = _jwtSettings.Value;
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            List<string> errors = new List<string>() { "Unauthorized" };
            return Result.Failure<string>(errors);
        }

        var roles = await _userManager.GetRolesAsync(user);

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

        return Result.Success(accessToken);
    }

    public async Task<Result> UpdateAsync(string id, UpdateRequest request, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Result.Cancelled();

        var validationResult = await _validatorUpdate.ValidateAsync(request, token);
        if (!validationResult.IsValid)
            return Result.ValidationFailure(new Dictionary<string, string[]>(validationResult.ToDictionary()));

        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id, token);

        if (user is null)
            return Result.Failure(new List<object>() { "There is no user with provided Id" });

        if (request.FirstName is not null)
            user.FirstName = request.FirstName;
        if (request.LastName is not null)
            user.LastName = request.LastName;
        if (request.UserName is not null)
            user.UserName = request.UserName;
        if (request.VehicleId is not null)
            user.VehicleId = request.VehicleId;
        if (request.PhoneNumber is not null)
            user.PhoneNumber = request.PhoneNumber;

        await _dbContext.SaveChangesAsync(token);
        
        return Result.Success();
    }

    public async Task<Result<PagedList<ApplicationUser>>> GetAllAsync(GetRequest request, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Result.Cancelled<PagedList<ApplicationUser>>();

        var validationResult = _validatorGet.Validate(request);
        if (!validationResult.IsValid)
            return Result.ValidationFailure<PagedList<ApplicationUser>>(new Dictionary<string, string[]>(validationResult.ToDictionary()));

        IQueryable<ApplicationUser> users = _userManager.Users;
        
        if (!string.IsNullOrEmpty(request.RoleName))
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(request.RoleName);
            users = usersInRole.AsQueryable();
        }
        
        users = users
            .Where(user =>
                (string.IsNullOrEmpty(request.Id) || user.Id == request.Id) &&
                (string.IsNullOrEmpty(request.UserName) || user.UserName.ToLower() == request.UserName.ToLower()) &&
                (string.IsNullOrEmpty(request.FirstName) || user.FirstName.ToLower() == request.FirstName.ToLower()) &&
                (string.IsNullOrEmpty(request.LastName) || user.LastName.ToLower() == request.LastName.ToLower()) &&
                (string.IsNullOrEmpty(request.Email) || user.Email.ToLower() == request.Email.ToLower()) &&
                (string.IsNullOrEmpty(request.PhoneNumber) || user.PhoneNumber == request.PhoneNumber) && 
                (!request.VehicleId.HasValue || user.VehicleId == request.VehicleId) && 
                (!request.CreatedAt.HasValue || (request.CreatedAt.HasValue && user.CreatedAt.Date == request.CreatedAt.Value.Date) )
            )
            .AsQueryable();
        
        var result = await Pagination.Paginate(users, request.pagination?.pageNumber, request.pagination?.pageSize, token);

        return Result.Success(result);
    }
    
    public async Task<Result> DeleteAsync(string id, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Result.Cancelled();
        
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id, token);

        if (user is null)
            return Result.Failure(new List<object>() { "There is no user with provided Id" });

        _dbContext.Users.Remove(user);

        await _dbContext.SaveChangesAsync(token);

        return Result.Success();
    }
}