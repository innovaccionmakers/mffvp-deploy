using Activations.Integrations.Affiliates;
using Activations.Integrations.Affiliates.CreateActivation;
using Activations.Integrations.Affiliates.UpdateAffiliate;
using Asp.Versioning;
using Common.SharedKernel.Presentation.Filters;
using Common.SharedKernel.Presentation.Results;
using Contributions.Integrations.FullContribution;
using MediatR;
using MFFVP.Api.Application.Activations;
using Microsoft.AspNetCore.Mvc;

namespace MFFVP.Api.BffWeb.Activations.Affiliates
{
    public sealed class AffiliatesEndpoints
    {
        private readonly IAffiliatesService _affiliatesService;

        public AffiliatesEndpoints(IAffiliatesService affiliatesService)
        {
            _affiliatesService = affiliatesService;
        }

        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var versionSet = app.NewApiVersionSet()
                    .HasApiVersion(new ApiVersion(1, 0))
                    .ReportApiVersions()
                    .Build();

            var group = app.MapGroup("bffWeb/api/v{version:apiVersion}/activations/activations")
                .WithTags("BFF Web - Activations")
                .WithApiVersionSet(versionSet)
                .HasApiVersion(1, 0)
                .WithOpenApi();

            group.MapGet("GetAll", async (ISender sender) =>
            {
                var result = await _affiliatesService.GetAffiliatesAsync(sender);
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .MapToApiVersion(1, 0)
            .Produces<IReadOnlyCollection<AffiliateResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapGet("GetById/{id}", async (int id, ISender sender) =>
            {
                var result = await _affiliatesService.GetAffiliateAsync(id, sender);
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .MapToApiVersion(1, 0)
            .Produces<AffiliateResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapPost("Create", async ([FromBody] CreateActivationCommand request, ISender sender) =>
            {
                var result = await _affiliatesService.CreateAffiliateAsync(request, sender);
                return result.ToApiResult();
            })
            .MapToApiVersion(1, 0)
            .AddEndpointFilter<TechnicalValidationFilter<CreateActivationCommand>>()
            .Produces<AffiliateResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapPut("Update", async ([FromBody] UpdateAffiliateCommand command, ISender sender) =>
            {
                var result = await _affiliatesService.UpdateAffiliateAsync(command, sender);
                return result.ToApiResult();
            })
            .MapToApiVersion(1, 0)
            .AddEndpointFilter<TechnicalValidationFilter<UpdateAffiliateCommand>>()
            .Produces<AffiliateResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapDelete("Delete/{id}", async (int id, ISender sender) =>
            {
                var result = await _affiliatesService.DeleteAffiliateAsync(id, sender);
                return result.Match(() => Results.NoContent(), ApiResults.Problem);
            })
            .MapToApiVersion(1, 0)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
        }
    }
}