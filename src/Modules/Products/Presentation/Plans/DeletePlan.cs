using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Products.Integrations.Plans.DeletePlan;

namespace Products.Presentation.Plans
{
    internal sealed class DeletePlan : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("plans/{id:long}", async (long id, ISender sender) =>
            {
                var result = await sender.Send(new DeletePlanCommand(id));
                return result.Match(
                    () => Results.NoContent(),
                    ApiResults.Problem
                );
            })
            .WithTags(Tags.Plans);
        }
    }
}