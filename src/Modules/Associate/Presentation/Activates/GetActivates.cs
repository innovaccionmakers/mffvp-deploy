using Associate.Integrations.Activates;
using Associate.Integrations.Activates.GetActivates;
using Common.SharedKernel.Presentation.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Associate.Presentation.Activates;

internal sealed class GetActivates : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {        
        app.MapGet("GetAssociates", async (ISender sender) =>
            {
                var result = await sender.Send(new GetActivatesQuery());
                return result.Value;
            })
            .Produces<IReadOnlyCollection<ActivateResponse>>()
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}