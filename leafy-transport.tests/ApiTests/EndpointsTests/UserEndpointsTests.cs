using System.Net;
using System.Text.Json;
using FluentAssertions;
using leafy_transport.api.Data;
using leafy_transport.api.Endpoints.User;

namespace leafy_transport.tests.ApiTests.EndpointsTests;

public class UserEndpointsTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;
    private readonly ApplicationDbContext _dbContext;
    private const string BaseUrl = "/api/v1";
    public UserEndpointsTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _dbContext = factory.CreateDbContext();
    }

    [Fact]
    public async Task TokenCheck_ShouldReturnStatusOK_WhenAuth()
    {
        var response = await _client.GetAsync($"{BaseUrl}/users/token-check");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostUsers_ShouldReturnStatusOK_WhenUserCreated()
    {
        var request = new RegisterRequest("test@test.test", "test@L123", "test@test.test", "test@test.test", "Employee");
        var content = new StringContent(JsonSerializer.Serialize(request), 
            System.Text.Encoding.UTF8,
            "application/json");
        
        var response = await _client.PostAsync($"{BaseUrl}/users", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("aaa", "ddd")]
    public async Task Login_ShouldReturnValidationFailedMessage_WhenValidationFailed(string email, string password)
    {
        var request = new LoginRequest(email, password);
        var content = new StringContent(JsonSerializer.Serialize(request),
            System.Text.Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync($"{BaseUrl}/users/login", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.Should().Contain("Validation failed");
    }
}