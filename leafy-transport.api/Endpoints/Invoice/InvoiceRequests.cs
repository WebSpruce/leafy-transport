namespace leafy_transport.api.Endpoints.Invoice;

public record CreateRequest(string InvoiceNumber, Guid ClientId, Guid? VehicleId , string Status, Guid? ParentInvoiceId);
public record GetRequest(Guid? Id, string? InvoiceNumber, Guid? ClientId, Guid? VehicleId , string? Status, Guid? ParentInvoiceId, PaginationRequest? pagination);
public record UpdateRequest(string? InvoiceNumber, Guid? ClientId, Guid? VehicleId , string? Status, Guid? ParentInvoiceId);