using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using leafy_transport.api.Data;
using leafy_transport.api.Endpoints.User;
using leafy_transport.api.Repositories.User;
using leafy_transport.models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace leafy_transport.tests.ApiTests.RepositoryTests;

public class UserRepositoryTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IValidator<RegisterRequest>> _validator;
    private readonly UserRepository _repository;
    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _dbContext = new ApplicationDbContext(options);
        _userManagerMock = MockUserManager();
        _validator = new Mock<IValidator<RegisterRequest>>();

        _repository = new UserRepository(_dbContext, _userManagerMock.Object, _validator.Object);
    }
    [Fact]
    public async Task RegisterUserAsync_ReturnsCancelled_WhenTokenCancelled()
    {
        var cancelTokenSource = new CancellationTokenSource();
        await cancelTokenSource.CancelAsync();
        var token = cancelTokenSource.Token;
        var request = new RegisterRequest("","","","","");

        var result = await _repository.RegisterUserAsync(request, token);

        result.IsCancelled.Should().BeTrue();
    }
    [Theory]
    [InlineData("Email", "Email is empty")]
    [InlineData("Password", "Password is empty")]
    public async Task RegisterUserAsync_ReturnsValidationErrorWithEmail_WhenValidationFailsOnEmail(
        string propertyName, string errorMessage)
    {
        var token = CancellationToken.None;
        var request = new RegisterRequest("","","","","");
        var validationFail = new ValidationResult(new List<ValidationFailure>()
        {
            new ValidationFailure(propertyName, errorMessage)
        });
        _validator.Setup(v => v.Validate(request)).Returns(validationFail);
        
        var result = await _repository.RegisterUserAsync(request, token);

        result.IsValidationFailure.Should().BeTrue();
        result.ValidationErrors.Should().ContainKey(propertyName);
    }
    [Fact]
    public async Task RegisterUserAsync_ReturnsFailure_WhenUserCreationFails()
    {
        var token = CancellationToken.None;
        var request = new RegisterRequest("","","","","");
        var validResult = new ValidationResult();
        _validator.Setup(v => v.Validate(request)).Returns(validResult);
        _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Creation failed" }));
            
        var result = await _repository.RegisterUserAsync(request, token);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e is IdentityError && ((IdentityError)e).Description == "Creation failed");
    }
    [Fact]
    public async Task RegisterUserAsync_ShouldReturnFailure_WhenAddToRoleFails()
    {
        var request = new RegisterRequest("test@example.com", "pass", "test@example.com", "test@example.com", "Employee");
        var token = CancellationToken.None;

        _validator.Setup(v => v.Validate(request)).Returns(new ValidationResult());

        _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(um => um.AddToRoleAsync(It.IsAny<ApplicationUser>(), request.Role))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "AddToRole failed" }));

        var result = await _repository.RegisterUserAsync(request, token);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e is IdentityError && ((IdentityError)e).Description == "AddToRole failed");
    }
    private static Mock<UserManager<ApplicationUser>> MockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(store.Object,
            null, null, null, null, null, null, null, null);
    }
}