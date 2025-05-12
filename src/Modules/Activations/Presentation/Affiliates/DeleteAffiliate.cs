using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Activations.Integrations.Affiliates.DeleteAffiliate;

namespace Activations.Presentation.Affiliates
{
    internal sealed class DeleteAffiliate : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("affiliates/{id:int}", async (int id, ISender sender) =>
            {
                var result = await sender.Send(new DeleteAffiliateCommand(id));
                return result.Match(
                    () => Results.NoContent(),
                    ApiResults.Problem
                );
            })
            .WithTags(Tags.Affiliates);
        }
    }
}