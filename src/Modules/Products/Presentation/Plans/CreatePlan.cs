using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Products.Integrations.Plans.CreatePlan;

namespace Products.Presentation.Plans;

internal sealed class CreatePlan : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("plans", async (Request request, ISender sender) =>
            {
                var result = await sender.Send(new CreatePlanCommand(
                    request.Name,
                    request.Description
                ));

                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.Plans);
    }

    internal sealed class Request
    {
        public string Name { get; init; }
        public string Description { get; init; }
    }
}