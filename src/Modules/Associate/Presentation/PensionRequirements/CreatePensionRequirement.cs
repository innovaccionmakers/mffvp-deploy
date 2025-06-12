using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Associate.Integrations.PensionRequirements.CreatePensionRequirement;
using Microsoft.AspNetCore.Mvc;
using Common.SharedKernel.Presentation.Filters;
using Associate.Integrations.PensionRequirements;

namespace Associate.Presentation.PensionRequirements
{
    internal sealed class CreatePensionRequirement : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {            
            app.MapPost("PensionRequirements", async ([FromBody] CreatePensionRequirementCommand request, ISender sender) =>
            {
                var result = await sender.Send(request);;
                return result.ToApiResult(result.Description);
            })
            .AddEndpointFilter<TechnicalValidationFilter<CreatePensionRequirementCommand>>()
            .Produces<PensionRequirementResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
        }
    }
}