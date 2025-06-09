using MediatR;
using Associate.Integrations.PensionRequirements.UpdatePensionRequirement;
using Associate.Integrations.PensionRequirements;
using Common.SharedKernel.Presentation.Results;
using Microsoft.AspNetCore.Mvc;
using MFFVP.Api.Application.Associate;
using Associate.Integrations.PensionRequirements.CreatePensionRequirement;
using Common.SharedKernel.Presentation.Filters;

namespace MFFVP.Api.BffWeb.Associate.PensionRequirements
{
    public sealed class PensionRequirementsEndpoints
    {
        private readonly IPensionRequirementsService _pensionrequirementsService;

        public PensionRequirementsEndpoints(IPensionRequirementsService pensionrequirementsService)
        {
            _pensionrequirementsService = pensionrequirementsService;
        }

        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("BffWeb/api/FVP/Associate")
                .WithTags("BFF Web - PensionRequirements")
                .WithOpenApi();

            group.MapGet("GetPensionRequirements", async (ISender sender) =>
            {
                var result = await _pensionrequirementsService.GetPensionRequirementsAsync(sender);
                return result;
            })
            .Produces <IReadOnlyCollection<PensionRequirementResponse>>()
            .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapPost("PensionRequirements", async ([FromBody] CreatePensionRequirementCommand request, ISender sender) =>
            {
                var result = await _pensionrequirementsService.CreatePensionRequirementAsync(request, sender);
                return result.ToApiResult(result.Description);
            })
            .AddEndpointFilter<TechnicalValidationFilter<CreatePensionRequirementCommand>>()
            .Produces<PensionRequirementResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapPut("PutPensionRequirements", async ([FromBody] UpdatePensionRequirementCommand command, ISender sender) =>
            {
                var result = await _pensionrequirementsService.UpdatePensionRequirementAsync(command, sender);
                return result.ToApiResult(result.Description);
            })
            .AddEndpointFilter<TechnicalValidationFilter<UpdatePensionRequirementCommand>>()
            .Produces<PensionRequirementResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
        }
    }
}