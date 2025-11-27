namespace leafy_transport.api.Endpoints.Vehicle;

public record CreateRequest(Guid CompanyId, string Type, double MaxWeight, string Status);
public record GetRequest(Guid? Id, Guid CompanyId, string? Type, double? MaxWeight, string? Status, PaginationRequest? pagination);
public record UpdateRequest(Guid CompanyId, string? Type, double? MaxWeight, string? Status);