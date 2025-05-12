using Asp.Versioning;
using Common.SharedKernel.Presentation.Filters;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using MFFVP.Api.Application.Trusts;
using Microsoft.AspNetCore.Mvc;
using Trusts.Integrations.FullContribution;

namespace MFFVP.Api.BffWeb.Trusts.FullContributions;

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

                    return result.ToApiResult();
                })
            .MapToApiVersion(1, 0)
            .AddEndpointFilter<TechnicalValidationFilter<CreateFullContributionCommand>>()
            .Produces<FullContributionResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}