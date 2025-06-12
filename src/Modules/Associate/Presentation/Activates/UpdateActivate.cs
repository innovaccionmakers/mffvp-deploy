using Associate.Integrations.Activates;
using Common.SharedKernel.Presentation.Results;
using Associate.Integrations.Activates.UpdateActivate;
using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Filters;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Presentation.Activates
{
    internal sealed class UpdateActivate : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("PutAssociate", async ([FromBody] UpdateActivateCommand request, ISender sender) =>
                {
                    var result = await sender.Send(request);            
                    return result.ToApiResult(result.Description);
                })
                .AddEndpointFilter<TechnicalValidationFilter<UpdateActivateCommand>>()
                .Produces<ActivateResponse>()
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status500InternalServerError);
        }
    }
}