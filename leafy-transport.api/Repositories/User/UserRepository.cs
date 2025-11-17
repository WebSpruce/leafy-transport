using System.Security.Claims;
using System.Text;
using FluentValidation;
using leafy_transport.api.Data;
using leafy_transport.api.Endpoints.User;
using leafy_transport.api.Interfaces.User;
using leafy_transport.models.Models;
using Microsoft.AspNetCore.Identity;
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
    private readonly IOptions<JwtSettings> _jwtSettings;

    public UserRepository(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, 
        IValidator<RegisterRequest> validator, IValidator<LoginRequest> validatorLogin, IOptions<JwtSettings> jwtSettings)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _validator = validator;
        _validatorLogin = validatorLogin;
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

    public async Task<Result> LoginAsync(LoginRequest request, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Result.Cancelled();

        var validationResult = _validatorLogin.Validate(request);
        if (!validationResult.IsValid)
            return Result.ValidationFailure(new Dictionary<string, string[]>(validationResult.ToDictionary()));
        
        var config = _jwtSettings.Value;
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            List<string> errors = new List<string>() { "Unauthorized" };
            return Result.Failure(errors);
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

        return Result.Success(new List<object>(){accessToken});
    }
}