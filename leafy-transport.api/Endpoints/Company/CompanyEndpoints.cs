using leafy_transport.api.Interfaces;
using leafy_transport.api.Interfaces.Company;
using leafy_transport.models.Models;

namespace leafy_transport.api.Endpoints.Company;

public class CompanyEndpoints : IModule
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        var companies = app.MapGroup(ApiRoutes.Companies.GroupName)
            .WithTags("Companies");

        companies.MapPost("", async (
            CreateRequest request,
            ICompanyRepository companyRepository,
            CancellationToken token
            ) =>
        {
            var result = await companyRepository.CreateAsync(request, token);

            if (result.IsCancelled)
                return Results.StatusCode(499);
            
            if (result.IsValidationFailure)
            {
                var problems = new HttpValidationProblemDetails(result.ValidationErrors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed",
                    Detail = "Validation errors occurred",
                    Instance = "/companies"
                };
                return Results.Problem(problems);
            }
            
            if (result.Errors?.Any() == true)
                return Results.NotFound(result.Errors?.FirstOrDefault());
            
            if (!result.IsSuccess)
                return Results.BadRequest(result.Errors);

            return Results.Ok(result.Value);
        }).RequireAuthorization();
        
        companies.MapGet("", async (
            Guid? id, 
            string? name, 
            string? slug,  
            string? ownerId, 
            int? page,
            int? pageSize,
            ICompanyRepository companyRepository,
            CancellationToken token) =>
        {
            var request = new GetRequest(id, name, slug, ownerId, new PaginationRequest(page, pageSize));
            var result = await companyRepository.GetAsync(request, token);
            
            if (result.IsCancelled)
                return Results.StatusCode(499);

            if (result.IsValidationFailure)
            {
                var problem = new HttpValidationProblemDetails(result.ValidationErrors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed",
                    Detail = "Validation errors occurred",
                    Instance = $"/companies"
                };
                return Results.Problem(problem);
            }
            
            if (result.Errors?.Any() == true)
                return Results.NotFound(result.Errors?.FirstOrDefault());

            return Results.Ok(result.Value);
        }).RequireAuthorization();
        
        companies.MapPatch("/{id}", async (
            Guid id,
            UpdateRequest request,
            ICompanyRepository companyRepository,
            CancellationToken token
        ) =>
        {
            var result = await companyRepository.UpdateAsync(id, request, token);

            if (result.IsCancelled)
                return Results.StatusCode(499);

            if (result.IsValidationFailure)
            {
                var problem = new HttpValidationProblemDetails(result.ValidationErrors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed",
                    Detail = "Validation errors occurred",
                    Instance = $"/companies/{id}"
                };
                return Results.Problem(problem);
            }

            if (result.Errors?.Any() == true)
                return Results.NotFound(result.Errors?.FirstOrDefault());

            return Results.Ok();
        }).RequireAuthorization();
        
        companies.MapDelete("/{id}", async (
            Guid id,
            string userId,
            ICompanyRepository companyRepository,
            CancellationToken token
        ) =>
        {
            var result = await companyRepository.DeleteAsync(id, userId, token);

            if (result.IsCancelled)
                return Results.StatusCode(499);

            if (result.Errors?.Any() == true)
                return Results.NotFound(result.Errors?.FirstOrDefault());

            return Results.Ok();
        }).RequireAuthorization();
    }
}