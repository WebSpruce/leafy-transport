namespace leafy_transport.api.Endpoints.User;

public record RegisterRequest(string Email, string Password, string FirstName, string LastName, string Role);
public record LoginRequest(string Email, string Password);
public record GetRequest(string? Id, string? Email, string? FirstName, string? LastName, string? UserName, string? PhoneNumber, Guid? VehicleId, string? RoleName, DateTime? CreatedAt, PaginationRequest? pagination);
public record UpdateRequest(string? FirstName, string? LastName, string? UserName, Guid? VehicleId, string? PhoneNumber);