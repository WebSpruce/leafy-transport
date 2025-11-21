using System.Text;
using FluentValidation;
using leafy_transport.api.Data;
using leafy_transport.api.Endpoints;
using leafy_transport.api.Infrastructure;
using leafy_transport.api.Interfaces.Invoice;
using leafy_transport.api.Interfaces.Product;
using leafy_transport.api.Interfaces.User;
using leafy_transport.api.Interfaces.Vehicle;
using leafy_transport.api.Repositories.Invoice;
using leafy_transport.api.Repositories.Product;
using leafy_transport.api.Repositories.User;
using leafy_transport.api.Repositories.Vehicle;
using leafy_transport.models.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

namespace leafy_transport.api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddOpenApi();

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("PGConnection"));
        });
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly, includeInternalTypes:true);

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();
        
        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
        var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters.ValidIssuer = jwtSettings?.Issuer;
                options.TokenValidationParameters.ValidAudience = jwtSettings?.Audience;
                options.TokenValidationParameters.IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.SecretKey!));
            });
        builder.Services.AddAuthorization();

        builder.Services.RegisterModules();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
        builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        builder.Services.AddScoped<IProductRepository, ProductRepository>();

        var app = builder.Build();
        
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(options =>
            {
                options.WithTitle("leafy-transport")
                    .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
            });

            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            if (dbContext.Database.IsRelational())
            {
                dbContext.Database.Migrate();
            }
        }

        app.UseHttpsRedirection();

        app.UseExceptionHandler();

        app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapEndpoints();
        
        app.Run();
    }
}