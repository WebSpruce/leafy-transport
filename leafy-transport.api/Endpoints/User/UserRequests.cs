namespace leafy_transport.api.Endpoints.User;

public record RegisterRequest(string Email, string Password, string FirstName, string LastName, string Role);
public record LoginRequest(string Email, string Password);