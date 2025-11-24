namespace leafy_transport.api.Endpoints.Client;

public record CreateRequest(string City, string Address, string PostCode, string Location);
public record GetRequest(Guid? Id, string? City, string? Address, string? PostCode, string? Location, PaginationRequest? pagination);
public record UpdateRequest(string? City, string? Address, string? PostCode, string? Location);