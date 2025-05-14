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

        var group = app.MapGroup("/bffWeb/contributions/full-contributions")
            .WithTags("BFF Web - FullContributions")
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