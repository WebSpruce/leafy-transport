using System.ComponentModel.DataAnnotations;

namespace leafy_transport.models.Models;

public class Client
{
    public required Guid Id { get; set; } = Guid.NewGuid();
    public required string City { get; set; }
    [MaxLength(255)]
    public required string Address { get; set; }
    [MaxLength(50)]
    public required string Postcode { get; set; }
    public required string Location { get; set; }
    public List<ApplicationUser> Users { get; set; } = [];
}