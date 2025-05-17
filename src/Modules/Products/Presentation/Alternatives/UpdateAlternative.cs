using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Products.Integrations.Alternatives.UpdateAlternative;

namespace Products.Presentation.Alternatives;

internal sealed class UpdateAlternative : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("alternatives/{id:long}", async (long id, Request request, ISender sender) =>
            {
                var command = new UpdateAlternativeCommand(
                    id,
                    request.NewAlternativeTypeId,
                    request.NewName,
                    request.NewStatus,
                    request.NewDescription
                );

                var result = await sender.Send(command);
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.Alternatives);
    }

    internal sealed class Request
    {
        public int NewAlternativeTypeId { get; set; }
        public string NewName { get; set; }
        public string NewStatus { get; set; }
        public string NewDescription { get; set; }
    }
}