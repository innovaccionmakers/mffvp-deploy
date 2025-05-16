using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Products.Integrations.Plans.UpdatePlan;

namespace Products.Presentation.Plans
{
    internal sealed class UpdatePlan : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("plans/{id:long}", async (long id, Request request, ISender sender) =>
            {
                var command = new UpdatePlanCommand(
                    id,
                    request.NewName, 
                    request.NewDescription
                );

                var result = await sender.Send(command);
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.Plans);
        }

        internal sealed class Request
        {
            public string NewName { get; set; }
            public string NewDescription { get; set; }
        }
    }
}