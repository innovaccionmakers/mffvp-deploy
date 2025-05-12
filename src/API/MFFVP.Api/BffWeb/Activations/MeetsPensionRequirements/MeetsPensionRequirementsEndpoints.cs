using MediatR;
using Activations.Integrations.MeetsPensionRequirements.UpdateMeetsPensionRequirement;
using Activations.Integrations.MeetsPensionRequirements;
using Common.SharedKernel.Presentation.Results;
using Microsoft.AspNetCore.Mvc;
using MFFVP.Api.Application.Activations;
using Activations.Integrations.MeetsPensionRequirements.CreateMeetsPensionRequirement;
using Asp.Versioning;

namespace MFFVP.Api.BffWeb.Activations.MeetsPensionRequirements
{
    public sealed class MeetsPensionRequirementsEndpoints
    {
        private readonly IMeetsPensionRequirementsService _meetspensionrequirementsService;

        public MeetsPensionRequirementsEndpoints(IMeetsPensionRequirementsService meetspensionrequirementsService)
        {
            _meetspensionrequirementsService = meetspensionrequirementsService;
        }

        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var versionSet = app.NewApiVersionSet()
                    .HasApiVersion(new ApiVersion(1, 0))
                    .ReportApiVersions()
                    .Build();

            var group = app.MapGroup("bffWeb/api/v{version:apiVersion}/activations/meetspensionrequirements")
                .WithTags("BFF Web - MeetsPensionRequirements")
                .WithApiVersionSet(versionSet)
                .HasApiVersion(1, 0)
                .WithOpenApi();

            group.MapGet("GetAll", async (ISender sender) =>
            {
                var result = await _meetspensionrequirementsService.GetMeetsPensionRequirementsAsync(sender);
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .MapToApiVersion(1, 0)
            .Produces<IReadOnlyCollection<MeetsPensionRequirementResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapGet("GetById/{id}", async (int id, ISender sender) =>
            {
                var result = await _meetspensionrequirementsService.GetMeetsPensionRequirementAsync(id, sender);
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .MapToApiVersion(1, 0)
            .Produces<MeetsPensionRequirementResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapPost("Create", async ([FromBody] CreateMeetsPensionRequirementCommand request, ISender sender) =>
            {
                var result = await _meetspensionrequirementsService.CreateMeetsPensionRequirementAsync(request, sender);
                return result.Match(() => Results.Ok(), ApiResults.Problem);
            })
            .MapToApiVersion(1, 0)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapPut("Update", async ([FromBody] UpdateMeetsPensionRequirementCommand command, ISender sender) =>
            {
                var result = await _meetspensionrequirementsService.UpdateMeetsPensionRequirementAsync(command, sender);
                return result.Match(() => Results.Ok(), ApiResults.Problem);
            })
            .MapToApiVersion(1, 0)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapDelete("Delete/{id}", async (int id, ISender sender) =>
            {
                var result = await _meetspensionrequirementsService.DeleteMeetsPensionRequirementAsync(id, sender);
                return result.Match(() => Results.NoContent(), ApiResults.Problem);
            })
            .MapToApiVersion(1, 0)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
        }
    }
}