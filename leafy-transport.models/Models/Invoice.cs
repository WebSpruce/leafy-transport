using System.ComponentModel.DataAnnotations;

namespace leafy_transport.models.Models;

public class Invoice
{
    public required Guid Id { get; set; }
    [MaxLength(100)]
    public required string InvoiceNumber { get; set; }
    public required int TotalWeight { get; set; }
    public required int TotalQuantity { get; set; }
    public required Guid ClientId { get; set; }
    public Guid? VehicleId { get; set; }
    [MaxLength(30)]
    public required string Status { get; set; }
    public Guid? ParentInvoiceId { get; set; }
}