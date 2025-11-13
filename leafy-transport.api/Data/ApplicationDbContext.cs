using leafy_transport.models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace leafy_transport.api.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceItem> InvoiceItems { get; set; }
    public DbSet<Product> Products { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<IdentityRole>().HasData(
            new IdentityRole { Id = "00000000-1111-0000-0000-000000000001", Name = models.Models.Roles.Admin, NormalizedName = "ADMIN" },
            new IdentityRole { Id = "00000000-2222-0000-0000-000000000002", Name = models.Models.Roles.Manager, NormalizedName = "MANAGER" },
            new IdentityRole { Id = "00000000-3333-0000-0000-000000000003", Name = models.Models.Roles.Employee, NormalizedName = "EMPLOYEE" }
        );
    }
}