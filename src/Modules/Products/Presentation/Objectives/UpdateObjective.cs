using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Products.Integrations.Objectives.UpdateObjective;

namespace Products.Presentation.Objectives
{
    internal sealed class UpdateObjective : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("objectives/{id:long}", async (long id, Request request, ISender sender) =>
            {
                var command = new UpdateObjectiveCommand(
                    id,
                    request.NewObjectiveTypeId, 
                    request.NewAffiliateId, 
                    request.NewAlternativeId, 
                    request.NewName, 
                    request.NewStatus, 
                    request.NewCreationDate
                );

                var result = await sender.Send(command);
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.Objectives);
        }

        internal sealed class Request
        {
            public int NewObjectiveTypeId { get; set; }
            public int NewAffiliateId { get; set; }
            public int NewAlternativeId { get; set; }
            public string NewName { get; set; }
            public string NewStatus { get; set; }
            public DateTime NewCreationDate { get; set; }
        }
    }
}