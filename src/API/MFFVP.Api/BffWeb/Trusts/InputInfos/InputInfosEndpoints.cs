using Common.SharedKernel.Presentation.Results;
using MediatR;
using MFFVP.Api.Application.Trusts;
using Microsoft.AspNetCore.Mvc;
using Trusts.Integrations.InputInfos;
using Trusts.Integrations.InputInfos.CreateInputInfo;
using Trusts.Integrations.InputInfos.UpdateInputInfo;

namespace MFFVP.Api.BffWeb.Trusts.InputInfos;

public sealed class InputInfosEndpoints
{
    private readonly IInputInfosService _inputinfosService;

    public InputInfosEndpoints(IInputInfosService inputinfosService)
    {
        _inputinfosService = inputinfosService;
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("bffWeb/api/contributions/inputinfos")
            .WithTags("BFF Web - InputInfos")
            .WithOpenApi();

        group.MapGet("GetAll", async (ISender sender) =>
            {
                var result = await _inputinfosService.GetInputInfosAsync(sender);
                return result.ToApiResult();
            })
            .Produces<IReadOnlyCollection<InputInfoResponse>>()
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("GetById/{id}", async (Guid id, ISender sender) =>
            {
                var result = await _inputinfosService.GetInputInfoAsync(id, sender);
                return result.ToApiResult();
            })
            .Produces<InputInfoResponse>()
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("Create", async ([FromBody] CreateInputInfoCommand request, ISender sender) =>
            {
                var result = await _inputinfosService.CreateInputInfoAsync(request, sender);
                return result.Match(() => Results.Ok(), ApiResults.Problem);
            })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPut("Update", async ([FromBody] UpdateInputInfoCommand command, ISender sender) =>
            {
                var result = await _inputinfosService.UpdateInputInfoAsync(command, sender);
                return result.Match(() => Results.Ok(), ApiResults.Problem);
            })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapDelete("Delete/{id}", async (Guid id, ISender sender) =>
            {
                var result = await _inputinfosService.DeleteInputInfoAsync(id, sender);
                return result.Match(() => Results.NoContent(), ApiResults.Problem);
            })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}