namespace leafy_transport.api.Endpoints.Products;

public record CreateRequest(string Name, int Weight, double Price);
public record GetRequest(Guid? Id, string? Name, int? Weight, double? Price, PaginationRequest? pagination);
public record UpdateRequest(string? Name, int? Weight, double? Price);