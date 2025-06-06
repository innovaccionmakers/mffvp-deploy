using MediatR;
using Trusts.Integrations.Trusts.UpdateTrust;
using Trusts.Integrations.Trusts;
using Common.SharedKernel.Presentation.Results;
using Microsoft.AspNetCore.Mvc;
using MFFVP.Api.Application.Trusts;
using Trusts.Integrations.Trusts.CreateTrust;

namespace MFFVP.Api.BffWeb.Trusts.Trusts
{
    public sealed class TrustsEndpoints
    {
        private readonly ITrustsService _trustsService;

        public TrustsEndpoints(ITrustsService trustsService)
        {
            _trustsService = trustsService;
        }

        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("bffWeb/FVP/trusts/trusts")
                .WithTags("BFF Web - Trusts")
                .WithOpenApi();

            group.MapGet("GetAll", async (ISender sender) =>
            {
                var result = await _trustsService.GetTrustsAsync(sender);
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .Produces<IReadOnlyCollection<TrustResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapGet("GetById/{trustId}", async (long trustId, ISender sender) =>
            {
                var result = await _trustsService.GetTrustAsync(trustId, sender);
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .Produces<TrustResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapPost("Create", async ([FromBody] CreateTrustCommand request, ISender sender) =>
            {
                var result = await _trustsService.CreateTrustAsync(request, sender);
                return result.Match(() => Results.Ok(), ApiResults.Problem);
            })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapPut("Update", async ([FromBody] UpdateTrustCommand command, ISender sender) =>
            {
                var result = await _trustsService.UpdateTrustAsync(command, sender);
                return result.Match(() => Results.Ok(), ApiResults.Problem);
            })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapDelete("Delete/{trustId}", async (long trustId, ISender sender) =>
            {
                var result = await _trustsService.DeleteTrustAsync(trustId, sender);
                return result.Match(() => Results.NoContent(), ApiResults.Problem);
            })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
        }
    }
}