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
            var group = app.MapGroup("bffWeb/api/associate/pensionrequirements")
                .WithTags("BFF Web - PensionRequirements")
                .WithOpenApi();

            group.MapGet("GetAll", async (ISender sender) =>
            {
                var result = await _pensionrequirementsService.GetPensionRequirementsAsync(sender);
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .Produces<IReadOnlyCollection<PensionRequirementResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapPost("Create", async ([FromBody] CreatePensionRequirementCommand request, ISender sender) =>
            {
                var result = await _pensionrequirementsService.CreatePensionRequirementAsync(request, sender);
                return result.ToApiResult();
            })
            .AddEndpointFilter<TechnicalValidationFilter<CreatePensionRequirementCommand>>()
            .Produces<PensionRequirementResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapPut("Update", async ([FromBody] UpdatePensionRequirementCommand command, ISender sender) =>
            {
                var result = await _pensionrequirementsService.UpdatePensionRequirementAsync(command, sender);
                return result.Match(() => Results.Ok(), ApiResults.Problem);
            })
            .AddEndpointFilter<TechnicalValidationFilter<UpdatePensionRequirementCommand>>()
            .Produces<PensionRequirementResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
        }
    }
}