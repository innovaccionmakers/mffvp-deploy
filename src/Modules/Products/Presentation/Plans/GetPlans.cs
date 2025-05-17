using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Products.Integrations.Plans.GetPlans;

namespace Products.Presentation.Plans;

internal sealed class GetPlans : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("plans", async (ISender sender) =>
            {
                var result = await sender.Send(new GetPlansQuery());
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.Plans);
    }
}