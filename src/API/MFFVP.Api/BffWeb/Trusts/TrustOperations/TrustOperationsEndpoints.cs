using Common.SharedKernel.Presentation.Results;
using MediatR;
using MFFVP.Api.Application.Trusts;
using Microsoft.AspNetCore.Mvc;
using Trusts.Integrations.TrustOperations;
using Trusts.Integrations.TrustOperations.CreateTrustOperation;
using Trusts.Integrations.TrustOperations.UpdateTrustOperation;

namespace MFFVP.Api.BffWeb.Trusts.TrustOperations;

public sealed class TrustOperationsEndpoints
{
    private readonly ITrustOperationsService _trustoperationsService;

    public TrustOperationsEndpoints(ITrustOperationsService trustoperationsService)
    {
        _trustoperationsService = trustoperationsService;
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("bffWeb/api/contributions/trustoperations")
            .WithTags("BFF Web - TrustOperations")
            .WithOpenApi();

        group.MapGet("GetAll", async (ISender sender) =>
            {
                var result = await _trustoperationsService.GetTrustOperationsAsync(sender);
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .Produces<IReadOnlyCollection<TrustOperationResponse>>()
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("GetById/{id}", async (Guid id, ISender sender) =>
            {
                var result = await _trustoperationsService.GetTrustOperationAsync(id, sender);
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .Produces<TrustOperationResponse>()
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("Create", async ([FromBody] CreateTrustOperationCommand request, ISender sender) =>
            {
                var result = await _trustoperationsService.CreateTrustOperationAsync(request, sender);
                return result.Match(() => Results.Ok(), ApiResults.Problem);
            })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPut("Update", async ([FromBody] UpdateTrustOperationCommand command, ISender sender) =>
            {
                var result = await _trustoperationsService.UpdateTrustOperationAsync(command, sender);
                return result.Match(() => Results.Ok(), ApiResults.Problem);
            })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapDelete("Delete/{id}", async (Guid id, ISender sender) =>
            {
                var result = await _trustoperationsService.DeleteTrustOperationAsync(id, sender);
                return result.Match(() => Results.NoContent(), ApiResults.Problem);
            })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}