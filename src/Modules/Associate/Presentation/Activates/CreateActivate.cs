using Associate.Integrations.Activates;
using Associate.Integrations.Activates.CreateActivate;
using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Filters;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Associate.Presentation.Activates;

internal sealed class CreateActivate : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("Activate", async ([FromBody] CreateActivateCommand request, ISender sender) =>
            {
                var result = await sender.Send(request);
                return result.ToApiResult(result.Description);
            })
            .AddEndpointFilter<TechnicalValidationFilter<CreateActivateCommand>>()
            .Produces<ActivateResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}