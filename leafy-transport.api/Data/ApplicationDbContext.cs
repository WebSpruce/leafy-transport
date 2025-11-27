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
    public DbSet<Company> Companies { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<IdentityRole>().HasData(
            new IdentityRole { Id = "00000000-1111-0000-0000-000000000001", Name = models.Models.Roles.Admin, NormalizedName = "ADMIN" },
            new IdentityRole { Id = "00000000-2222-0000-0000-000000000002", Name = models.Models.Roles.Manager, NormalizedName = "MANAGER" },
            new IdentityRole { Id = "00000000-3333-0000-0000-000000000003", Name = models.Models.Roles.Employee, NormalizedName = "EMPLOYEE" }
        );

        builder.Entity<Invoice>()
            .HasMany(i => i.InvoiceItems)
            .WithOne()
            .HasForeignKey(ii => ii.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Invoice>()
            .HasOne<Client>()
            .WithMany()
            .HasForeignKey(i => i.ClientId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<Invoice>()
            .HasOne<Vehicle>()
            .WithMany()
            .HasForeignKey(i => i.VehicleId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.Entity<Product>()
            .HasMany(p => p.InvoiceItems)
            .WithOne()
            .HasForeignKey(ii => ii.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<InvoiceItem>()
            .HasOne<Invoice>()
            .WithMany(i => i.InvoiceItems)
            .HasForeignKey(ii => ii.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<InvoiceItem>()
            .HasOne<Product>()
            .WithMany(p => p.InvoiceItems)
            .HasForeignKey(ii => ii.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<Company>()
            .HasOne(c => c.Owner)
            .WithMany()  
            .HasForeignKey(c => c.OwnerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Company>()
            .HasMany(c => c.Users)
            .WithOne()  
            .HasForeignKey(u => u.CompanyId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Company>()
            .HasMany(c => c.Clients)
            .WithOne()
            .HasForeignKey(cl => cl.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Company>()
            .HasMany(c => c.Vehicles)
            .WithOne()
            .HasForeignKey(v => v.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Company>()
            .HasMany(c => c.Invoices)
            .WithOne()
            .HasForeignKey(i => i.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Company>()
            .HasMany(c => c.Products)
            .WithOne()
            .HasForeignKey(p => p.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}