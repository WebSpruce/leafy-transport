namespace leafy_transport.api.Endpoints.Vehicle;

public record CreateRequest(string Type, double MaxWeight, string Status);
public record GetRequest(Guid? Id, string? Type, double? MaxWeight, string? Status);