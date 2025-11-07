using System.ComponentModel.DataAnnotations;

namespace leafy_transport.models.Models;

public class Vehicle
{
    public required Guid Id { get; set; } = Guid.NewGuid();
    [MaxLength(30)]
    public required string Type { get; set; }
    public required double MaxWeight { get; set; }
    [MaxLength(30)]
    public required string Status { get; set; }
}