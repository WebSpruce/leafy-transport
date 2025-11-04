namespace leafy_transport.models.Models;

public class InvoiceItem
{
    public required Guid Id { get; set; }
    public required Guid InvoiceId { get; set; }
    public required Guid ProductId { get; set; }
    public required int TotalWeight { get; set; }
    public required int Quantity { get; set; }
}