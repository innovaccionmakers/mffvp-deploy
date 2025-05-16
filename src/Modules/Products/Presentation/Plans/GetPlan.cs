using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Products.Integrations.Plans.GetPlan;

namespace Products.Presentation.Plans
{
    internal sealed class GetPlan : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("plans/{id:long}", async (long id, ISender sender) =>
            {
                var result = await sender.Send(new GetPlanQuery(id));
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.Plans);
        }
    }
}