using Contributions.Integrations.FullContribution;
using MediatR;
using MFFVP.Api.Application.Contributions;
using Common.SharedKernel.Presentation.Results;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace MFFVP.Api.BffWeb.Contributions.FullContributions
{
    public sealed class FullContributionsEndpoints
    {
        private readonly IFullContributionService _fullContributionService;

        public FullContributionsEndpoints(IFullContributionService fullContributionService)
        {
            _fullContributionService = fullContributionService;
        }

        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var versionSet = app.NewApiVersionSet()
                    .HasApiVersion(new ApiVersion(1, 0))
                    .ReportApiVersions()
                    .Build();

            var group = app.MapGroup("/v{version:apiVersion}/bffWeb/api/contributions/full-contributions")
                           .WithTags("BFF Web - FullContributions")
                           .WithApiVersionSet(versionSet)
                           .HasApiVersion(1, 0)
                           .WithOpenApi();

            group.MapPost("Create",
                async ([FromBody] CreateFullContributionCommand request, ISender sender) =>
                {
                    var result = await _fullContributionService
                        .CreateFullContributionAsync(request, sender);

                    if (result.IsSuccess)
                    {
                        return Results.Ok(result.Value);
                    }
                    else
                    {
                        return ApiResults.Problem(result);
                    }
                })
                .MapToApiVersion(1, 0)
                .Produces<FullContributionResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status500InternalServerError);
        }
    }
}
