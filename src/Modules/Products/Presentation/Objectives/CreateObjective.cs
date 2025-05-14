using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Products.Integrations.Objectives.CreateObjective;

namespace Products.Presentation.Objectives
{
    internal sealed class CreateObjective : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("objectives", async (Request request, ISender sender) =>
            {
                var result = await sender.Send(new CreateObjectiveCommand(
                    request.ObjectiveTypeId, 
                    request.AffiliateId, 
                    request.AlternativeId, 
                    request.Name, 
                    request.Status, 
                    request.CreationDate
                ));

                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.Objectives);
        }

        internal sealed class Request
        {
            public int ObjectiveTypeId { get; init; }
            public int AffiliateId { get; init; }
            public int AlternativeId { get; init; }
            public string Name { get; init; }
            public string Status { get; init; }
            public DateTime CreationDate { get; init; }
        }
    }
}