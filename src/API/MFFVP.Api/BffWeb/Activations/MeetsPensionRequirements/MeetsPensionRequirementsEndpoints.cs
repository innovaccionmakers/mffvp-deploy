using Activations.Integrations.MeetsPensionRequirements;
using Activations.Integrations.MeetsPensionRequirements.CreateMeetsPensionRequirement;
using Activations.Integrations.MeetsPensionRequirements.UpdateMeetsPensionRequirement;
using Asp.Versioning;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using MFFVP.Api.Application.Activations;
using Microsoft.AspNetCore.Mvc;

namespace MFFVP.Api.BffWeb.Activations.MeetsPensionRequirements;

public sealed class MeetsPensionRequirementsEndpoints
{
    private readonly IMeetsPensionRequirementsService _meetspensionrequirementsService;

    public MeetsPensionRequirementsEndpoints(IMeetsPensionRequirementsService meetspensionrequirementsService)
    {
        _meetspensionrequirementsService = meetspensionrequirementsService;
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {

        var group = app.MapGroup("bffWeb/activations/meetspensionrequirements")
            .WithTags("BFF Web - MeetsPensionRequirements")
            .WithOpenApi();

        group.MapGet("GetAll", async (ISender sender) =>
            {
                var result = await _meetspensionrequirementsService.GetMeetsPensionRequirementsAsync(sender);
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .MapToApiVersion(1, 0)
            .Produces<IReadOnlyCollection<MeetsPensionRequirementResponse>>()
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("GetById/{id}", async (int id, ISender sender) =>
            {
                var result = await _meetspensionrequirementsService.GetMeetsPensionRequirementAsync(id, sender);
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .MapToApiVersion(1, 0)
            .Produces<MeetsPensionRequirementResponse>()
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