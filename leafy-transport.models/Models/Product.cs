using System.ComponentModel.DataAnnotations;

namespace leafy_transport.models.Models;

public class Product
{
    public required Guid Id { get; set; } = Guid.NewGuid();
    [MaxLength(200)]
    public required string Name { get; set; }
    public required int TotalWeight { get; set; }
}